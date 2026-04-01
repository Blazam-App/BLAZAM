using Newtonsoft.Json;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace BLAZAM.Helpers
{
    /// <summary>
    /// Provides extension methods and utility functions for string manipulation and conversion.
    /// </summary>
    public static class StringHelpers
    {


        /// <summary>
        /// Generates a consistent hash code for a string that does not change with application restarts.
        /// </summary>
        /// <param name="input">The input string. If null, 0 is returned.</param>
        /// <returns>An integer hash code. Returns 0 if the input string is null.</returns>
        public static int GetAppHashCode(this string input)
        {
            if (input == null)
            {
                return 0;
            }
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                foreach (char c in input)
                {
                    hash = hash * 23 + c;
                }
                return hash;
            }
        }
        public static string AppTrim(this string str)
        {
            var trimmed = str.Trim();
            trimmed = trimmed.Trim('⠀');
            return trimmed;
        }
        /// <summary>
        /// Determines whether a string is null or an empty string.
        /// </summary>
        /// <param name="str">The string to test.</param>
        /// <returns>True if the string is null or empty; otherwise, false.</returns>
        public static bool IsNullOrEmpty(this string? str)
        {
            return string.IsNullOrEmpty(str); // System.String.IsNullOrEmpty handles both null and empty.
        }

        /// <summary>
        /// Checks if a URL is local to the host (relative path or localhost).
        /// </summary>
        /// <param name="url">The URL string to check.</param>
        /// <returns>True if the URL is local; otherwise, false.</returns>
        public static bool IsUrlLocalToHost(this string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return true;
            }
            if (url.StartsWith("https://localhost"))
            {
                return true;
            }
            // Original logic for empty string was handled by the IsNullOrEmpty check above.
            return url[0] == '/' && (url.Length == 1 ||
                    url[1] != '/' && url[1] != '\\') ||   // "/" or "/foo" but not "//" or "/\"
                    url.Length > 1 &&
                     url[0] == '~' && url[1] == '/';   // "~/" or "~/foo"
        }

        /// <summary>
        /// Creates a consistent GUID from a string input using MD5 hashing.
        /// </summary>
        /// <param name="input">The input string. Must not be null.</param>
        /// <returns>A <see cref="Guid"/> generated from the input string.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the input string is null.</exception>
        public static Guid ToGuid(this string input)
        {
            ArgumentNullException.ThrowIfNull(input);

            // Use MD5 hash to get a 16-byte hash of the string
            using MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(input));
            // Create a new Guid using the hash
            return new Guid(hash);
        }

        /// <summary>
        /// Converts a <see cref="SecureString"/> to its plain text representation.
        /// </summary>
        /// <param name="secureString">The SecureString to convert. If null, an empty string is returned.</param>
        /// <returns>The plain text string.</returns>
        public static string ToPlainText(this SecureString? secureString)
        {
            if (secureString == null)
            {
                return string.Empty;
            }

            IntPtr bstrPtr = Marshal.SecureStringToBSTR(secureString);
            try
            {
                var plainText = Marshal.PtrToStringBSTR(bstrPtr);
                if (plainText == null)
                {
                    plainText = string.Empty;
                }

                return plainText;
            }
            finally
            {
                Marshal.ZeroFreeBSTR(bstrPtr);
            }
        }

        /// <summary>
        /// Converts a plain text string to a <see cref="SecureString"/>.
        /// </summary>
        /// <param name="plainText">The plain text string to convert. If null, an empty SecureString is returned.</param>
        /// <returns>A SecureString representation of the input.</returns>
        public static SecureString ToSecureString(this string plainText)
        {
            if (plainText == null)
            {
                return new SecureString();
            }
            return new NetworkCredential("", plainText).SecurePassword;
        }

        /// <summary>
        /// Formats an LDAP OU path into a more human-readable, slash-delimited format (e.g., /TopOU/SubOU).
        /// </summary>
        /// <param name="ou">The LDAP OU string (e.g., OU=SubOU,OU=TopOU,DC=domain,DC=com).</param>
        /// <returns>A user-friendly OU path, or null if the input is null.</returns>
        public static string? ToPrettyOu(this string? ou)
        {
            if (ou == null)
            {
                return null;
            }

            var ouComponents = Regex.Matches(ou, @"OU=([^,]*)")
                .Select(m => m.Groups[1].Value)
                .ToList();
            ouComponents.Reverse();
            return "/" + string.Join("/", ouComponents);
        }

        /// <summary>
        /// Converts a Fully Qualified Domain Name (FQDN) into its equivalent Distinguished Name (DN) format.
        /// </summary>
        /// <param name="fqdn">The FQDN string (e.g., sub.domain.com). If null or empty, an empty string is returned.</param>
        /// <returns>The DN string (e.g., DC=sub,DC=domain,DC=com), or an empty string if input is null or empty.</returns>
        public static string FqdnToDN(this string fqdn)
        {
            if (string.IsNullOrEmpty(fqdn))
            {
                return string.Empty;
            }
            // Split the FQDN into its domain components
            string[] domainComponents = fqdn.Split('.');

            // Build the DN by appending each reversed domain component as a RDN (relative distinguished name)
            StringBuilder dnBuilder = new();
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
        public static T? FromJson<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }

}
