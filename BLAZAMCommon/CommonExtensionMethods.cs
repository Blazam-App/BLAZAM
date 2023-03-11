
using BLAZAM.Common.Data.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data.ActiveDirectory;
using BLAZAM.Common.Data.Database;
using BlazorTemplater;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.CodeDom;
using System.DirectoryServices;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Collections;
using System;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.TeamFoundation.Work.WebApi;
using Newtonsoft.Json.Linq;
using System.DirectoryServices.ActiveDirectory;
using System.IO.Compression;

namespace BLAZAM
{
    public static class CommonExtensions
    {
        /// <summary>
        /// Adds all files in a directory recursively to the zip archive
        /// </summary>
        /// <param name="archive"></param>
        /// <param name="directory"></param>
        /// <param name="zipBasePath">The root path within the archive
        /// to place the added files</param>
        /// <returns></returns>
        public static void AddToZip(this ZipArchive archive, SystemDirectory directory,string zipBasePath="")
        {
            // Loop through all files in the current directory
            foreach (var file in directory.Files)
            {
                // Create an entry for each file with its relative path
                ZipArchiveEntry entry = archive.CreateEntry(Path.Combine(zipBasePath, file.Name));

                // Copy the file contents to the entry stream
                using (FileStream fs = file.OpenReadStream())
                using (Stream es = entry.Open())
                {
                    fs.CopyTo(es);
                }
            }

            // Loop through all subdirectories in the current directory
            foreach (var sdi in directory.SubDirectories)
            {
                // Recursively add files and subdirectories with their relative paths
                AddToZip(archive, sdi, Path.Combine(zipBasePath, sdi.Name));
            }
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
                diff += BitCount((byte)(a[i] ^ b[i]));
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


        public static bool IsUrlLocalToHost(this string url)
        {
            if (url.StartsWith("https://localhost")) return true;
            if (url == "") return true;
            return ((url[0] == '/' && (url.Length == 1 ||
                    (url[1] != '/' && url[1] != '\\'))) ||   // "/" or "/foo" but not "//" or "/\"
                    (url.Length > 1 &&
                     url[0] == '~' && url[1] == '/'));   // "~/" or "~/foo"
        }

        public static string ToPlainText(this SecureString? secureString)
        {
            if (secureString == null) return String.Empty;
            IntPtr bstrPtr = Marshal.SecureStringToBSTR(secureString);
            try
            {
                var plainText = Marshal.PtrToStringBSTR(bstrPtr);
                if (plainText == null)
                    plainText = String.Empty;
                return plainText;

            }
            finally
            {
                Marshal.ZeroFreeBSTR(bstrPtr);
            }
        }
        public static SecureString ToSecureString(this string plainText)
        {
            return new NetworkCredential("", plainText).SecurePassword;
        }

        public static string? ToPrettyOu(this string? ou)
        {
            if (ou == null) return null;
            var ouComponents = Regex.Matches(ou, @"OU=([^,]*)")
                .Select(m => m.Groups[1].Value)
                .ToList();
            ouComponents.Reverse();
            return string.Join("/", ouComponents);
        }
        public static string FqdnToDN(this string fqdn)
        {
            // Split the FQDN into its domain components
            string[] domainComponents = fqdn.Split('.');



            // Build the DN by appending each reversed domain component as a RDN (relative distinguished name)
            StringBuilder dnBuilder = new StringBuilder();
            foreach (string dc in domainComponents)
            {
                dnBuilder.Append("DC=");
                dnBuilder.Append(dc);
                dnBuilder.Append(",");
            }

            // Remove the last comma
            dnBuilder.Length--;

            // Return the DN
            return dnBuilder.ToString();
        }
        public static string ToHex(this System.Drawing.Color color)
        {
            String rtn = String.Empty;
            try
            {
                rtn = "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2") + color.A.ToString("X2");
            }
            catch
            {
                //doing nothing
            }

            return rtn;
        }

        public static string ToSidString(this byte[] sid)
        {
            if (null == sid) return null;
            // Create a SecurityIdentifier object from the input byte array
            var securityIdentifier = new SecurityIdentifier(sid, 0);

            // Use the SecurityIdentifier object's Value property to get the string representation of the SID
            return securityIdentifier.Value;

        }


        public static byte[] ReizeRawImage(this byte[] rawImage, int maxDimension)
        {
            using (var image = Image.Load(rawImage))
            {
                if (image.Height > image.Width)
                    image.Mutate(x => x.Resize(0, maxDimension));
                else
                    image.Mutate(x => x.Resize(maxDimension, 0));

                using (var ms = new MemoryStream())
                {
                    image.SaveAsPng(ms);
                    return ms.ToArray();
                }
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
        public static IADsLargeInteger? DateTimeToAdsValue(this DateTime? value)
        {
            if (value == null) return null;
            try
            {

                long? fileTime = value?.ToFileTimeUtc();
                if (fileTime == null) return null;
                object fto = 0;
                IADsLargeInteger largeInt = new ADsLargeInteger();
                largeInt.HighPart = (int)(fileTime >> 32);
                largeInt.LowPart = (int)(fileTime & 0xffffffff);
                return largeInt;
            }
            catch
            {
                return null;
            }
        }
        public static DateTime? AdsValueToDateTime(this object value)
        {
            //read file time 133213804065419619
            try
            {

                IADsLargeInteger v = value as IADsLargeInteger;

                if (null == v) return DateTime.MinValue;

                long dV = ((long)v.HighPart << 32) + (long)v.LowPart;
                return DateTime.FromFileTime(dV);
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}
