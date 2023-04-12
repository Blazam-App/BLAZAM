using BLAZAM.ActiveDirectory.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BLAZAM.ActiveDirectory.Helpers
{
    public static class ActiveDirectoryHelpers
    {
     
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
            PropertyInfo[] properties;
            if (changed is not null)
                properties = changed.GetType().GetProperties();
            else
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
