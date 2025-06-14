using BLAZAM.Common.Data;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Collections;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;

namespace BLAZAM.Helpers
{
    public static class CommonHelpers
    {
        /// <summary>
        /// Rounds a number to the specified decimal places
        /// </summary>
        /// <param name="number"></param>
        /// <param name="decimalPlaces"></param>
        /// <returns></returns>
        public static double Round(this double number, int decimalPlaces = 0)
        {
            return Math.Round(number, decimalPlaces);
        }

        public static string GetValueChangesString(this List<AuditChangeLog> changes, Func<AuditChangeLog, object?> valueSelector)
        {
            var values = "";
            foreach (var c in changes)
            {
                string? value = "";

                if (valueSelector.Invoke(c) is IEnumerable<object> enumerable)
                {
                    foreach (var obj in enumerable)
                    {
                        value += obj.ToString() + ",";
                    }
                }
                else
                {
                    value = valueSelector.Invoke(c)?.ToString();
                }
                values += c.Field + "=" + value + ";";

            }
            return values;
        }

       public static string? GetEventProperty(this EventRecord eventRecord, int index)
        {
            try
            {
                return eventRecord.Properties[index].Value.ToString();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return null;
            }
        }
        public static List<AuditChangeLog> GetChanges(this object changed, object? original)
        {
            if (original == null)
            {
                return new List<AuditChangeLog>();

            }
            // Check if both objects are null or same reference
            if (ReferenceEquals(changed, original))
                return new List<AuditChangeLog>();


            // Check if both objects are of the same type if both were provided
            if (changed is not null && original is not null && changed.GetType() != original.GetType())
                throw new ArgumentException("Objects must be of the same type");

            var changes = BuildAuditChangeLog(changed, original);

            // Return the list of changes
            return changes;

        }
        public static object SetPropertyValue(this object obj, string propertyName, object value)
        {
            var props = obj.GetType().GetProperties();
            var matchingProp = props.FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase));
            if (matchingProp != null)
            {
                matchingProp.SetValue(obj, value);
            }
            else
            {
                Loggers.SystemLogger.Debug("No matching property in {@Object}{@PropertyName}", obj, propertyName);
            }
            return true;
        }
        public static object? GetPropertyValue(this object obj, string propertyName)
        {
            var props = obj.GetType().GetProperties();
            var matchingProp = props.FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase));
            var propertyValue = matchingProp?.GetValue(obj);
            return propertyValue;
        }
        public static bool PropertyValueEquals(this object obj, string propertyName, object? value)
        {
            var propertyValue = obj.GetPropertyValue(propertyName);
            if (propertyValue == null)
            {
                if (value == null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                var propertyStrVal = propertyValue.ToString();


                return propertyStrVal?.Equals(value?.ToString(), StringComparison.InvariantCultureIgnoreCase)==true;

            }
        }
        private static List<AuditChangeLog> BuildAuditChangeLog(object? changed, object? original = null)
        {
            List<AuditChangeLog> changes = new();
            // Get the properties of the object type
            PropertyInfo[] properties = new PropertyInfo[0];
            if (changed is not null)
                properties = changed.GetType().GetProperties();
            else if (original is not null)
                properties = original.GetType().GetProperties();

            // Iterate over each property
            foreach (var property in properties)
            {
                // Get the values of the property for both objects

                object? oldValue = null;
                if (original is not null)
                    oldValue = property.GetValue(original);
                object? newValue = null;
                if (changed is not null)
                    newValue = property.GetValue(changed);

                // Compare the values using Equals method
                if ((oldValue != null && newValue != null)
                    && (oldValue is not null && !oldValue.Equals(newValue)
                    || (newValue is not null && !newValue.Equals(oldValue))))
                {
                    // Create a new AuditChangeLog instance with the property name and values
                    var change = new AuditChangeLog
                    {
                        Field = property.Name,
                        OldValue = oldValue,
                        NewValue = newValue
                    };

                    // Add the change to the list
                    changes.Add(change);
                }
            }
            return changes;
        }




        /// <summary>
        /// Adds all files in a directory recursively to the zip archive
        /// </summary>
        /// <param name="archive"></param>
        /// <param name="directory"></param>
        /// <param name="basePath">The root path from where files are
        /// being added.
        /// </param>
        /// <returns></returns>
        public static void AddToZip(this ZipArchive archive, SystemDirectory directory, string basePath)
        {
            // Loop through all files in the current directory
            foreach (var file in directory.Files)
            {
                try
                {
                    using FileStream fs = file.OpenReadStream();
                    // Create an entry for each file with its relative path
                    ZipArchiveEntry entry = archive.CreateEntry(directory.FullPath.Replace(basePath, "") + file.Name + file.Extension);

                    // Copy the file contents to the entry stream


                    using (Stream es = entry.Open())
                    {
                        fs.CopyTo(es);
                    }
                }
                catch (Exception ex)
                {
                    Loggers.SystemLogger.Error(ex.Message + " {@Error}", ex);
                }
            }



            // Loop through all subdirectories in the current directory
            foreach (var sdi in directory.SubDirectories)
            {
                // Recursively add files and subdirectories with their relative paths
                archive.AddToZip(sdi, basePath);
            }
        }

        /// <summary>
        /// Saves this memory stream to a filesystem file
        /// </summary>
        /// <param name="memoryStream"></param>
        /// <param name="destinationFile"></param>
        public static void SaveTo(this MemoryStream memoryStream, SystemFile destinationFile)
        {
            if (destinationFile.Exists)
                destinationFile.Delete();


            using var outStream = destinationFile.OpenWriteStream();
            memoryStream.Seek(0, SeekOrigin.Begin);
            memoryStream.CopyTo(outStream);
            outStream.Close();
            memoryStream.Close();
        }


        public static bool IsNullOrEmpty(this ICollection collection)
        {
            return (collection == null || collection.Count < 1);
        }



        /// <summary>
        /// Resizes a raw byte array, assumed to be an image, to the maximum dimension provided
        /// </summary>
        /// <param name="rawImage"></param>
        /// <param name="maxDimension"></param>
        /// <param name="cropToSquare"></param>
        /// <returns></returns>
        public static byte[] ReizeRawImage(this byte[] rawImage, int maxDimension, bool cropToSquare = false)
        {
            using (var image = Image.Load(rawImage))
            {
                if (image.Height > image.Width)
                {
                    if (cropToSquare)
                        image.Mutate(x => x.Crop(image.Width, image.Width));
                    image.Mutate(x => x.Resize(0, maxDimension));

                }
                else
                {
                    if (cropToSquare)
                        image.Mutate(x => x.Crop(image.Height, image.Height));
                    image.Mutate(x => x.Resize(maxDimension, 0));
                }
                using (var ms = new MemoryStream())
                {
                    image.SaveAsPng(ms);
                    return ms.ToArray();
                }
            }
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {

            var enumerator = enumerable.GetEnumerator();
            while (enumerator.MoveNext())
            {
                action.Invoke(enumerator.Current);
            }

        }

        #region ADSI Extension Methods


        [
            ComImport,
            Guid("9068270b-0939-11d1-8be1-00c04fd8d503"),
            InterfaceType(ComInterfaceType.InterfaceIsIDispatch)
        ]
        public interface IADsLargeInteger
        {
            [DispId(2)] int HighPart { get; set; }
            [DispId(3)] int LowPart { get; set; }
        }
        public class ADsLargeInteger : IADsLargeInteger
        {
            public int HighPart { get; set; }
            public int LowPart { get; set; }
        }
        public static long? DateTimeToAdsValue(this DateTime? value)
        {
            if (value == null) return null;
            try
            {

                long fileTime = value.Value.ToUniversalTime().ToFileTimeUtc();
                return fileTime;

            }
            catch
            {
                return null;
            }
        }
        //133241760000000000
        //31029034
        //1743527936
        /// <summary>
        /// Converts an ADS datetime to a .Net <see cref="DateTime"/> in UTC
        /// </summary>
        /// <param name="value"></param>
        /// <returns>A UTC <see cref="DateTime"/></returns>
        public static DateTime? AdsValueToDateTime(this object value)
        {
            DateTime? dateTime = null;
            //read file time 133213804065419619
            try
            {
                if (value is DateTime) return (DateTime?)value;

                if (value == null) return null;


                Int64 longInt = Int64.MinValue;
                try
                {
                    Int64.TryParse(value.ToString(), out longInt);
                }
                catch (FormatException)
                {
                    //Ignore input string format exception because it's probably
                    // a com object.

                }
                if (longInt != Int64.MinValue && longInt != 0)
                {
                    dateTime = DateTime.FromFileTimeUtc(longInt);
                }
                else
                {
                    IADsLargeInteger? v = value as IADsLargeInteger;

                    if (null == v) return DateTime.MinValue;

                    long dV = ((long)v.HighPart << 32) + v.LowPart;


                    dateTime = DateTime.FromFileTimeUtc(dV);
                }
            }
            catch
            {
                return null;
            }
            if (dateTime == null || dateTime.Equals(ADS_NULL_TIME) || dateTime.Equals(DateTime.MinValue))
                dateTime = null;
            return dateTime;
        }

        public static DateTime ADS_NULL_TIME
        {
            get
            {
                var ads_null_time = DateTime.ParseExact("01/01/1601 12:00:00 AM", "MM/dd/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                return DateTime.SpecifyKind(ads_null_time, DateTimeKind.Utc);
            }
        }
        public static Guid? ToGuid(this byte[]? guidBytes)
        {
            if (null == guidBytes) return null;
            // Create a SecurityIdentifier object from the input byte array
            var guid = new Guid(guidBytes);

            // Use the SecurityIdentifier object's Value property to get the string representation of the SID
            return guid;

        }

        public static string? ToHexADString(this byte[]? byteArray)
        {
            if (null == byteArray) return null;
            // Create a SecurityIdentifier object from the input byte array
      var hexString = Convert.ToHexString(byteArray);
            var ldapString = ToLdapHexString(hexString);

            // Use the SecurityIdentifier object's Value property to get the string representation of the SID
            return ldapString;

        }
        private static string ToLdapHexString(string? hexString)
        {
            if (string.IsNullOrEmpty(hexString))
            {
                return string.Empty;
            }

            // Ensure the hex string has an even number of characters (pairs for bytes)
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException("Input hex string must have an even number of characters.", nameof(hexString));
            }

            // Pre-allocate capacity: original length + one backslash per byte
            StringBuilder ldapHex = new StringBuilder(hexString.Length + hexString.Length / 2);

            for (int i = 0; i < hexString.Length; i += 2)
            {
                ldapHex.Append('\\');
                ldapHex.Append(hexString[i]);       // Append the first hex char of the pair
                ldapHex.Append(hexString[i + 1]);   // Append the second hex char of the pair
                                                    // Or combine: ldapHex.Append(hexString.Substring(i, 2));
            }

            return ldapHex.ToString();
        }
        /// <summary>
        /// Converts a Security Identifier (SID) from a byte array to its string format (e.g., "S-1-5-21-...").
        /// This method is cross-platform and does not rely on the Windows-specific SecurityIdentifier class.
        /// </summary>
        /// <param name="sid">The byte array representing the SID.</param>
        /// <returns>The string representation of the SID, or an empty string if the input is null or invalid.</returns>
        public static string ToSidString(this byte[]? sid)
        {
            if (sid == null || sid.Length < 8)
            {
                return "";
            }

            var builder = new StringBuilder("S-");

            // Revision Level (1 byte)
            builder.Append(sid[0]);

            // Sub-Authority Count (1 byte)
            var subAuthorityCount = sid[1];

            // Identifier Authority (6 bytes, big-endian)
            long identifierAuthority = 0;
            for (var i = 2; i <= 7; i++)
            {
                identifierAuthority = (identifierAuthority << 8) + sid[i];
            }
            builder.Append('-').Append(identifierAuthority);

            // Sub-Authorities (4 bytes each, little-endian)
            var offset = 8;
            for (var i = 0; i < subAuthorityCount; i++)
            {
                // BitConverter handles the little-endian conversion correctly on most architectures.
                uint subAuthority = BitConverter.ToUInt32(sid, offset);
                builder.Append('-').Append(subAuthority);
                offset += 4;
            }

            return builder.ToString();
        }
        /// <summary>
        /// Converts a Security Identifier (SID) from its string format (e.g., "S-1-5-21-...") to a byte array.
        /// This method is cross-platform and does not rely on the Windows-specific SecurityIdentifier class.
        /// </summary>
        /// <param name="sidString">The string representation of the SID.</param>
        /// <returns>The byte array representing the SID.</returns>
        /// <exception cref="FormatException">Thrown if the SID string is not in a valid format.</exception>
        public static byte[] ToSidByteArray(this string? sidString)
        {
            if (string.IsNullOrEmpty(sidString))
            {
                return Array.Empty<byte>();
            }

            try
            {
                string[] parts = sidString.Split('-');

                // A valid SID string must start with 'S-' and have at least three parts (S, Rev, IdentifierAuthority)
                if (parts.Length < 3 || !parts[0].Equals("S", StringComparison.OrdinalIgnoreCase))
                {
                    throw new FormatException("Invalid SID string format.");
                }

                int subAuthorityCount = parts.Length - 3;
                // Calculate the required byte array size: 8 bytes for the header + 4 bytes for each sub-authority
                byte[] sidBytes = new byte[8 + subAuthorityCount * 4];

                // 1. Write Revision Level (1 byte)
                sidBytes[0] = byte.Parse(parts[1]);

                // 2. Write Sub-Authority Count (1 byte)
                sidBytes[1] = (byte)subAuthorityCount;

                // 3. Write Identifier Authority (6 bytes, big-endian)
                long identifierAuthority = long.Parse(parts[2]);
                for (int i = 5; i >= 0; i--)
                {
                    // Place the bytes from right to left
                    sidBytes[2 + i] = (byte)(identifierAuthority & 0xFF);
                    identifierAuthority >>= 8;
                }

                // 4. Write Sub-Authorities (4 bytes each, little-endian)
                for (int i = 0; i < subAuthorityCount; i++)
                {
                    uint subAuthority = uint.Parse(parts[3 + i]);
                    // BitConverter correctly handles little-endian conversion on most architectures
                    byte[] subAuthorityBytes = BitConverter.GetBytes(subAuthority);
                    // Copy the 4 bytes into the correct position in the main byte array
                    Array.Copy(subAuthorityBytes, 0, sidBytes, 8 + i * 4, 4);
                }

                return sidBytes;
            }
            catch (Exception ex) when (ex is not FormatException)
            {
                // Catch parsing errors or other exceptions and wrap them in a FormatException
                throw new FormatException("The SID string could not be parsed.", ex);
            }
        }

        #endregion
    }
}
