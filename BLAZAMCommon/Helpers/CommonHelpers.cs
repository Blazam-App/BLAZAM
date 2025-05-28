using BLAZAM.Common.Data;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System; // Added
using System.Collections;
using System.Collections.Generic; // Added
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO; // Added for MemoryStream, Stream
using System.IO.Compression;
using System.Linq; // Added for FirstOrDefault
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;

namespace BLAZAM.Helpers
{
    /// <summary>
    /// Provides common helper and extension methods for various operations.
    /// </summary>
    public static class CommonHelpers
    {
        /// <summary>
        /// Rounds a double-precision floating-point number to a specified number of fractional digits.
        /// </summary>
        /// <param name="number">The number to round.</param>
        /// <param name="decimalPlaces">The number of decimal places in the return value. Defaults to 0.</param>
        /// <returns>The number rounded to the specified number of decimal places.</returns>
        public static double Round(this double number, int decimalPlaces = 0)
        {
            return Math.Round(number, decimalPlaces);
        }

        /// <summary>
        /// Formats a list of audit changes into a string, extracting values using a selector.
        /// </summary>
        /// <param name="changes">The list of audit changes.</param>
        /// <param name="valueSelector">A function to select the value from an <see cref="AuditChangeLog"/>.</param>
        /// <returns>A semicolon-separated string of field=value pairs representing the changes.</returns>
        public static string GetValueChangesString(this List<AuditChangeLog> changes, Func<AuditChangeLog, object?> valueSelector)
        {
            if (changes == null)
            {
                return string.Empty;
            }
            if (valueSelector == null)
            {
                return string.Empty;
            }

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

        /// <summary>
        /// Retrieves a property value from an <see cref="EventRecord"/> by its index.
        /// </summary>
        /// <param name="eventRecord">The event record.</param>
        /// <param name="index">The zero-based index of the property.</param>
        /// <returns>The string representation of the property value, or null if the property is not found or an error occurs.</returns>
        public static string? GetEventProperty(this EventRecord? eventRecord, int index)
        {
            if (eventRecord == null)
            {
               return null;
            }
            try
            {
                return eventRecord.Properties[index].Value.ToString();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Compares two objects and returns a list of properties that have changed.
        /// </summary>
        /// <param name="changed">The object with new values.</param>
        /// <param name="original">The original object to compare against.</param>
        /// <returns>A list of <see cref="AuditChangeLog"/> detailing the changes. Returns an empty list if no changes or if original is null.</returns>
        /// <exception cref="ArgumentException">Thrown if 'changed' and 'original' are of different types (and neither is null).</exception>
        public static List<AuditChangeLog> GetChanges(this object changed, object? original)
        {
            if (original == null)
            {
                // If original is null, any property in 'changed' is considered a new value, but our current BuildAuditChangeLog handles this.
                // If 'changed' is also null, ReferenceEquals handles it.
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

        /// <summary>
        /// Sets a property's value on an object using reflection, ignoring case for property name.
        /// </summary>
        /// <param name="obj">The object whose property to set.</param>
        /// <param name="propertyName">The name of the property to set.</param>
        /// <param name="value">The value to set the property to.</param>
        /// <returns>True if the property was found and set; false otherwise (e.g., if obj is null or property not found).</returns>
        public static bool SetPropertyValue(this object? obj, string propertyName, object value)
        {
            if (obj == null)
            {
                return false;
            }
            var props = obj.GetType().GetProperties();
            var matchingProp = props.FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase));
            if (matchingProp != null)
            {
                matchingProp.SetValue(obj, value);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a property's value from an object using reflection, ignoring case for property name.
        /// </summary>
        /// <param name="obj">The object from which to get the property value.</param>
        /// <param name="propertyName">The name of the property to get.</param>
        /// <returns>The value of the property, or null if the object is null or the property is not found.</returns>
        public static object? GetPropertyValue(this object? obj, string propertyName)
        {
            if (obj == null)
            {
                return null;
            }
            var props = obj.GetType().GetProperties();
            var matchingProp = props.FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase));
            var propertyValue = matchingProp?.GetValue(obj);
            return propertyValue;
        }

        /// <summary>
        /// Compares a property's value on an object with a given value, ignoring case for string comparison.
        /// </summary>
        /// <param name="obj">The object whose property to compare.</param>
        /// <param name="propertyName">The name of the property to compare.</param>
        /// <param name="value">The value to compare against.</param>
        /// <returns>True if the property value equals the given value (case-insensitive for strings); false otherwise, or if obj is null.</returns>
        public static bool PropertyValueEquals(this object? obj, string propertyName, object? value)
        {
            if (obj == null)
            {
               return false; // If obj is null, it cannot have a property that equals value (unless value is also null, but this simplifies)
            }
            var propertyValue = obj.GetPropertyValue(propertyName);
            if (propertyValue == null)
            {
                return value == null;
            }
            else
            {
                var propertyStrVal = propertyValue.ToString();
                return propertyStrVal?.Equals(value?.ToString(), StringComparison.InvariantCultureIgnoreCase) == true;
            }
        }

        /// <summary>
        /// Compares properties of two objects and builds a list of changes.
        /// </summary>
        /// <param name="changed">The object with potentially new values.</param>
        /// <param name="original">The original object. If null, all properties in 'changed' are treated as new.</param>
        /// <returns>A list of <see cref="AuditChangeLog"/> detailing the differences.</returns>
        private static List<AuditChangeLog> BuildAuditChangeLog(object? changed, object? original = null)
        {
            List<AuditChangeLog> changes = new();
            PropertyInfo[] properties;

            if (changed != null)
                properties = changed.GetType().GetProperties();
            else if (original != null)
                properties = original.GetType().GetProperties();
            else
                return changes; // Both are null, no properties to compare

            foreach (var property in properties)
            {
                object? oldValue = original != null ? property.GetValue(original) : null;
                object? newValue = changed != null ? property.GetValue(changed) : null;

                // Determine if there's a change
                bool isChanged = false;
                if (oldValue == null && newValue != null) isChanged = true;
                else if (oldValue != null && newValue == null) isChanged = true;
                else if (oldValue != null && newValue != null && !oldValue.Equals(newValue)) isChanged = true;
                // Note: if both are null, they are considered not changed.

                if (isChanged)
                {
                    var change = new AuditChangeLog
                    {
                        Field = property.Name,
                        OldValue = oldValue,
                        NewValue = newValue
                    };
                    changes.Add(change);
                }
            }
            return changes;
        }

        /// <summary>
        /// Adds all files in a directory recursively to the zip archive.
        /// </summary>
        /// <param name="archive">The zip archive to add files to.</param>
        /// <param name="directory">The directory whose contents to add.</param>
        /// <param name="basePath">The root path from where files are being added, used to determine relative paths in the archive.</param>
        public static void AddToZip(this ZipArchive archive, SystemDirectory directory, string basePath)
        {
            if (archive == null)
            {
                return;
            }
            if (directory == null)
            {
                return;
            }

            foreach (var file in directory.Files)
            {
                if (file == null)
                {
                    continue;
                }
                try
                {
                    using FileStream fs = file.OpenReadStream();
                    ZipArchiveEntry entry = archive.CreateEntry(directory.FullPath.Replace(basePath, "") + file.Name + file.Extension);
                    using (Stream es = entry.Open())
                    {
                        fs.CopyTo(es);
                    }
                }
                catch (Exception)
                {
                }
            }

            foreach (var sdi in directory.SubDirectories)
            {
                archive.AddToZip(sdi, basePath);
            }
        }

        /// <summary>
        /// Saves a MemoryStream to a specified file system file.
        /// </summary>
        /// <param name="memoryStream">The memory stream to save.</param>
        /// <param name="destinationFile">The system file to save the stream to.</param>
        public static void SaveTo(this MemoryStream memoryStream, SystemFile destinationFile)
        {
            if (memoryStream == null)
            {
               return;
            }
            if (destinationFile == null)
            {
                return;
            }

            if (destinationFile.Exists)
                destinationFile.Delete();

            using var outStream = destinationFile.OpenWriteStream();
            memoryStream.Seek(0, SeekOrigin.Begin);
            memoryStream.CopyTo(outStream);
            // Closing outStream is handled by using statement.
            // memoryStream.Close(); // Generally, the caller should manage the lifecycle of the passed-in stream.
        }

        /// <summary>
        /// Checks if an <see cref="ICollection"/> is null or has no elements.
        /// </summary>
        /// <param name="collection">The collection to check.</param>
        /// <returns>True if the collection is null or empty; false otherwise.</returns>
        public static bool IsNullOrEmpty(this ICollection collection)
        {
            return (collection == null || collection.Count < 1);
        }

        /// <summary>
        /// Resizes a raw byte array, assumed to be an image, to a specified maximum dimension, optionally cropping to a square.
        /// </summary>
        /// <param name="rawImage">The byte array containing the image data.</param>
        /// <param name="maxDimension">The maximum dimension (width or height) for the resized image.</param>
        /// <param name="cropToSquare">If true, the image is cropped to a square before resizing. Defaults to false.</param>
        /// <returns>A byte array of the resized image in PNG format, or an empty byte array if the input is null.</returns>
        public static byte[] ResizeRawImage(this byte[] rawImage, int maxDimension, bool cropToSquare = false) // Renamed
        {
            if (rawImage == null)
            {
                return Array.Empty<byte>();
            }
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

        /// <summary>
        /// Executes an action for each item in an enumerable collection.
        /// </summary>
        /// <typeparam name="T">The type of elements in the enumerable.</typeparam>
        /// <param name="enumerable">The enumerable collection.</param>
        /// <param name="action">The action to execute for each item.</param>
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            if (enumerable == null)
            {
                return;
            }
            if (action == null)
            {
                return;
            }

            var enumerator = enumerable.GetEnumerator();
            while (enumerator.MoveNext())
            {
                action.Invoke(enumerator.Current);
            }
        }

        #region ADSI Extension Methods

        /// <summary>
        /// Represents the ADSI LargeInteger structure, used for date/time and other large integer values in Active Directory.
        /// This interface is used for COM interop with ADSI.
        /// </summary>
        [
            ComImport,
            Guid("9068270b-0939-11d1-8be1-00c04fd8d503"),
            InterfaceType(ComInterfaceType.InterfaceIsIDispatch)
        ]
        public interface IADsLargeInteger
        {
            /// <summary>Gets or sets the high part of the large integer.</summary>
            [DispId(2)] int HighPart { get; set; }
            /// <summary>Gets or sets the low part of the large integer.</summary>
            [DispId(3)] int LowPart { get; set; }
        }

        /// <summary>
        /// A managed representation of the ADSI IADsLargeInteger, primarily for testing or scenarios where COM interop is not directly available.
        /// </summary>
        public class ADsLargeInteger : IADsLargeInteger
        {
            /// <summary>Gets or sets the high part of the large integer.</summary>
            public int HighPart { get; set; }
            /// <summary>Gets or sets the low part of the large integer.</summary>
            public int LowPart { get; set; }
        }

        /// <summary>
        /// Converts a .NET DateTime? to an ADSI LargeInteger compatible long value (FILETIME UTC).
        /// </summary>
        /// <param name="value">The nullable DateTime to convert. If null, returns null.</param>
        /// <returns>A long representing the FILETIME UTC, or null.</returns>
        public static long? DateTimeToAdsValue(this DateTime? value)
        {
            if (value == null) return null;
            try
            {
                var maxFileTime = DateTime.Parse("Sunday, November 16, 4769 9:46:40 AM Z");
                if (value > maxFileTime)
                {
                    return null;
                }
                long fileTime = value.Value.ToUniversalTime().ToFileTimeUtc();
                return fileTime;
            }
            catch (Exception) // More specific exception handling could be added if needed
            {
                return null;
            }
        }

        /// <summary>
        /// Converts an ADSI date/time object (either a long representing FILETIME UTC or an IADsLargeInteger) to a nullable .NET DateTime in UTC.
        /// </summary>
        /// <param name="value">The ADSI date/time object.</param>
        /// <returns>A nullable DateTime in UTC, or null if conversion fails or the ADSI value represents a null/zero time.</returns>
        public static DateTime? AdsValueToDateTime(this object? value)
        {
            DateTime? dateTime = null;
            try
            {
                if (value is DateTime dtValue) return dtValue; // Already a DateTime
                if (value == null) return null;

                Int64 longInt = Int64.MinValue;
              
                    // Attempt to parse if it's a string representation of a long
                    if (value is string s && long.TryParse(s, out long parsedLong))
                    {
                        longInt = parsedLong;
                    }
                    else if (value is IConvertible convertibleValue) // Handle other numeric types
                    {
                         longInt = convertibleValue.ToInt64(CultureInfo.InvariantCulture);
                    }
               


                if (longInt != Int64.MinValue && longInt != 0)
                {
                    dateTime = DateTime.FromFileTimeUtc(longInt);
                }
                else
                {
                    IADsLargeInteger? v = value as IADsLargeInteger;
                    if (v != null)
                    {
                        long dV = ((long)v.HighPart << 32) + v.LowPart;
                        if (dV != 0) // Avoid converting 0 FILETIME to 1601/01/01 if it represents "no date"
                            dateTime = DateTime.FromFileTimeUtc(dV);
                       
                    }
                   
                }
            }
            catch (Exception)
            {
                return null; // Return null on any other unexpected error
            }

            if (dateTime == null || dateTime.Equals(ADS_NULL_TIME) || dateTime.Equals(DateTime.MinValue))
                dateTime = null; // Standardize "null" or "zero" AD dates to null .NET DateTime
            return dateTime;
        }

        /// <summary>
        /// Represents the "null" or earliest possible date in ADSI time (January 1, 1601, 12:00:00 AM UTC).
        /// </summary>
        public static DateTime ADS_NULL_TIME
        {
            get
            {
                // Use TryParseExact for robustness if needed, but this format is standard.
                var ads_null_time = DateTime.ParseExact("01/01/1601 12:00:00 AM", "MM/dd/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                return DateTime.SpecifyKind(ads_null_time, DateTimeKind.Utc);
            }
        }

        /// <summary>
        /// Converts a byte array representing a GUID to a <see cref="Guid"/> object.
        /// </summary>
        /// <param name="guidBytes">The byte array to convert. Can be null.</param>
        /// <returns>A nullable Guid. Returns null if guidBytes is null.</returns>
        public static Guid? ToGuid(this byte[]? guidBytes)
        {
            if (null == guidBytes) return null;
            try
            {
                return new Guid(guidBytes);
            }
            catch (ArgumentException) // Handles cases where byte array is not 16 bytes
            {
                return null;
            }
        }

        /// <summary>
        /// Converts a byte array to a hexadecimal string prefixed for AD where each byte is represented by \XX.
        /// </summary>
        /// <param name="byteArray">The byte array to convert. Can be null.</param>
        /// <returns>An LDAP-compatible hex string, or null if byteArray is null.</returns>
        public static string? ToHexADString(this byte[]? byteArray)
        {
            if (null == byteArray) return null;
            var hexString = Convert.ToHexString(byteArray);
            return ToLdapHexString(hexString);
        }

        /// <summary>
        /// Converts a standard hex string to an LDAP-compatible hex string (e.g., "A1B2" becomes "\A1\B2").
        /// </summary>
        /// <param name="hexString">The hex string to convert.</param>
        /// <returns>An LDAP-formatted hex string, or an empty string if input is null or empty.</returns>
        /// <exception cref="ArgumentException">Thrown if the input hex string has an odd number of characters.</exception>
        private static string ToLdapHexString(string? hexString)
        {
            if (string.IsNullOrEmpty(hexString))
            {
                return string.Empty;
            }

            if (hexString.Length % 2 != 0)
            {
                // Log this as it indicates a programming error or unexpected data
                throw new ArgumentException("Input hex string must have an even number of characters.", nameof(hexString));
            }

            StringBuilder ldapHex = new StringBuilder(hexString.Length + hexString.Length / 2);
            for (int i = 0; i < hexString.Length; i += 2)
            {
                ldapHex.Append('\\');
                ldapHex.Append(hexString[i]);
                ldapHex.Append(hexString[i + 1]);
            }
            return ldapHex.ToString();
        }

        /// <summary>
        /// Converts a byte array representing a Security Identifier (SID) to its string format.
        /// </summary>
        /// <param name="sid">The byte array SID. Can be null.</param>
        /// <returns>The string representation of the SID, or an empty string if sid is null.</returns>
        public static string ToSidString(this byte[]? sid)
        {
            if (null == sid) return "";
            try
            {
                var securityIdentifier = new SecurityIdentifier(sid, 0);
                return securityIdentifier.Value;
            }
            catch (ArgumentException) // Handles invalid SID byte arrays
            {
                return ""; // Or throw, depending on desired error handling
            }
        }

        /// <summary>
        /// Converts a string representation of a Security Identifier (SID) to its byte array format.
        /// </summary>
        /// <param name="sidString">The SID string. Can be null or empty.</param>
        /// <returns>The byte array representation of the SID, or an empty array if sidString is null or empty.</returns>
        public static byte[] ToSidByteArray(this string sidString)
        {
            if (string.IsNullOrEmpty(sidString)) return Array.Empty<byte>();
            try
            {
                var securityIdentifier = new SecurityIdentifier(sidString);
                byte[] sidBytes = new byte[securityIdentifier.BinaryLength];
                securityIdentifier.GetBinaryForm(sidBytes, 0);
                return sidBytes;
            }
            catch (ArgumentException) // Handles invalid SID string formats
            {
                return Array.Empty<byte>(); // Or throw
            }
        }
        #endregion
    }
}
