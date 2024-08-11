using BLAZAM.Common.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
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


        public static void SaveTo(this MemoryStream memoryStream, SystemFile destinationFile)
        {
            if (destinationFile.Exists)
                destinationFile.Delete();


            using var outStream = destinationFile.OpenWriteStream();
            memoryStream.Seek(0,SeekOrigin.Begin);
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
        public static PropertyInfo GetPropertyFromExpression<T>(this T obj, Expression<Func<T, object>> GetPropertyLambda)
        {
            MemberExpression Exp = null;

            //this line is necessary, because sometimes the expression comes in as Convert(originalexpression)
            if (GetPropertyLambda.Body is UnaryExpression)
            {
                var UnExp = (UnaryExpression)GetPropertyLambda.Body;
                if (UnExp.Operand is MemberExpression)
                {
                    Exp = (MemberExpression)UnExp.Operand;
                }
                else
                    throw new ArgumentException();
            }
            else if (GetPropertyLambda.Body is MemberExpression)
            {
                Exp = (MemberExpression)GetPropertyLambda.Body;
            }
            else
            {
                throw new ArgumentException();
            }

            return (PropertyInfo)Exp.Member;
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
                    Int64.TryParse(value.ToString(),out longInt);
                }
                catch (FormatException)
                {
                    //Ignore input string format exception because it's probably
                    // a com object.

                }
                if (longInt != Int64.MinValue && longInt!=0)
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

        public static string ToSidString(this byte[] sid)
        {
            if (null == sid) return "";
            // Create a SecurityIdentifier object from the input byte array
            var securityIdentifier = new SecurityIdentifier(sid, 0);

            // Use the SecurityIdentifier object's Value property to get the string representation of the SID
            return securityIdentifier.Value;

        }


        #endregion
    }
}
