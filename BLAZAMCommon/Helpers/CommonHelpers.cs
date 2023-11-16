using BLAZAM.Common.Data;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Helpers
{
    public static class CommonHelpers
    {
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

        public static List<AuditChangeLog> GetChanges(this object changed, object original)
        {
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
                if (oldValue is null || newValue is null
                    || !Equals(oldValue, newValue))
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
                    ZipArchiveEntry entry = archive.CreateEntry(directory.Path.Replace(basePath, "") + "/" + file.Name + file.Extension);

                    // Copy the file contents to the entry stream


                    using (Stream es = entry.Open())
                    {
                        fs.CopyTo(es);
                    }
                }
                catch
                {

                }
            }



            // Loop through all subdirectories in the current directory
            foreach (var sdi in directory.SubDirectories)
            {
                // Recursively add files and subdirectories with their relative paths
                archive.AddToZip(sdi, basePath);
            }
        }

        public static bool IsNullOrEmpty(this ICollection collection)
        {
            return (collection == null || collection.Count < 1);
        }


        // A method that returns the number of different bits between two byte arrays
        public static int BitDifference(this byte[] a, byte[] b)
        {
            // Check that the arrays have the same length
            if (a.Length != b.Length) throw new ArgumentException("Arrays must have the same length");

            // Initialize a counter for different bits
            int diff = 0;

            // Loop through each byte in the arrays
            for (int i = 0; i < a.Length; i++)
            {
                // XOR the bytes and count the number of 1s in the result
                diff += ((byte)(a[i] ^ b[i])).BitCount();
            }

            // Return the total number of different bits
            return diff;
        }

        // A method that returns the number of 1s in a byte using Brian Kernighan's algorithm
        public static int BitCount(this byte n)
        {
            // Initialize a counter for 1s
            int count = 0;

            // Loop until n becomes zero
            while (n > 0)
            {
                // Clear the least significant bit set to 1 and increment the counter
                n &= (byte)(n - 1);
                count++;
            }

            // Return the number of 1s in n
            return count;
        }

        public static byte[] ToByteArray(this int number, int? length = null)
        {
            byte[] byteArray = BitConverter.GetBytes(number);

            // Check endianness and reverse byte array if necessary
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(byteArray);
            }
            if (length != null)
                // Pad the byte array to the desired length with zeroes
                Array.Resize(ref byteArray, (int)length);

            return byteArray;
        }





        public static string ToSidString(this byte[] sid)
        {
            if (null == sid) return "";
            // Create a SecurityIdentifier object from the input byte array
            var securityIdentifier = new SecurityIdentifier(sid, 0);

            // Use the SecurityIdentifier object's Value property to get the string representation of the SID
            return securityIdentifier.Value;

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
                action.Invoke((T)enumerator.Current);
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

                long? fileTime = value?.ToUniversalTime().ToFileTimeUtc();
                if (fileTime == null) return null;
                return fileTime;
                //object fto = 0;
                //IADsLargeInteger largeInt = new ADsLargeInteger();

                //largeInt.HighPart = (int)(fileTime >> 32);
                //largeInt.LowPart = (int)(fileTime & 0xFFFFFFFF);

                //return largeInt;
            }
            catch
            {
                return null;
            }
        }
        //133241760000000000
        //31029034
        //1743527936
        public static DateTime? AdsValueToDateTime(this object value)
        {
            DateTime? dateTime = null;
            //read file time 133213804065419619
            try
            {
                if (value == null) return null;

                if (value.GetType().FullName != "System.__ComObject")
                {
                    Int64? longInt = null;
                    try
                    {
                        longInt = Int64.Parse(value.ToString());
                    }
                    catch (FormatException)
                    {
                        //Ignore input string format exception because it's probably
                        // a com object.

                    }

                    if (longInt != null)
                    {
                        dateTime = DateTime.FromFileTimeUtc(longInt.Value);
                    }
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

        #endregion
    }
}
