using System.Management;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace BLAZAM.Helpers
{
    public static class DirectoryTools
    {
        public static T? GetPropertyValue<T>(this ManagementObject? mo, string propertyName)
        {
            var value = mo.GetPropertyValue(propertyName);
            if (value is T) { return (T)value; }
            return default;
            try
            {
                return (T)value;
            }
            catch
            {
                return default;
            }
        }
        public static string FqdnToDn(string fqdn)
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

        public static string? DnToOu(string? dN)
        {
            if (dN == null) return null;
            var ouComponents = Regex.Matches(dN, @"OU=([^,]+)")
                            .Select(m => m.Value)
                            .ToList();

            return string.Join(",", ouComponents);
        }

        public static string? ParentOU(string? dN)
        {
            return dN.Substring(dN.IndexOf("OU="));
        }

        public static string? PrettifyOu(string? ou)
        {
            if (ou == null) return null;
            var ouComponents = Regex.Matches(ou, @"OU=([^,]*)")
                .Select(m => m.Groups[1].Value)
                .ToList();
            ouComponents.Reverse();
            return string.Join("/", ouComponents);
        }


    }
}
