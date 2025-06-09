using BLAZAM.ActiveDirectory;
using BLAZAM.ActiveDirectory.Adapters;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using BLAZAM.Database.Models;
using BLAZAM.Database.Models.Notifications;
using BLAZAM.Database.Models.Permissions;
using BLAZAM.Database.Models.Templates;
using BLAZAM.Logger;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices.Protocols;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace BLAZAM.Helpers
{
    public static class ActiveDirectoryHelpers
    {
        /// <summary>
        /// Returns true if the domain controller is reachable by this web
        /// server, otherwise returns false.
        /// </summary>
        /// <param name="dc">The Domain Controller to test</param>
        /// <returns></returns>
        public static bool IsPingable(this DomainController dc)
        {
            return NetworkTools.PingHost(dc.IPAddress);
        }
        public static IServiceCollection AddActiveDirectoryServices(this IServiceCollection services)
        {
            //Provide a primary Active Directory connection as a service
            //We run this as a singleton so each user connection doesn't have to wait for connection verification to happen
            services.AddSingleton<IActiveDirectoryContext, ActiveDirectoryContext>();

            //Provide a per-user Active Directory connection as a service
            services.AddSingleton<IActiveDirectoryContextFactory, ActiveDirectoryContextFactory>();

            services.AddScoped<ScopedActiveDirectoryContext>();

            return services;
        }

        public static IEnumerable<IDirectoryEntryAdapter> MoveToTop(this IEnumerable<IDirectoryEntryAdapter> enumerable, Func<IDirectoryEntryAdapter, bool> matchingPredicate)
        {
            var list = enumerable.ToList();
            if (list.Count() < 1) return list;
            List<IDirectoryEntryAdapter> mathingItems = new();
            for (int x = 0; x < list.Count(); x++)
            {

                if (matchingPredicate.Invoke(list[x]))
                {
                    var toMove = list[x];
                    list.RemoveAt(x);
                    x--;
                    mathingItems.Add(toMove);

                }
            }
            if (mathingItems.Count > 0)
            {
                list.InsertRange(0, mathingItems.OrderBy(x => x.CanonicalName));
            }
            return list.AsEnumerable();
        }


        /// <summary>
        /// Converts a FQDN to it's DN equivalent
        /// </summary>
        /// <param name="fqdn"></param>
        /// <returns></returns>
        public static string FqdnToDn(string fqdn)
        {
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
        /// <summary>
        /// Populates the fields of the provided <see cref="IADUser"/>
        /// with the values set within this <see cref="DirectoryTemplate"/>
        /// </summary>
        /// <param name="template">This template</param>
        /// <param name="user">The user to set the template fields for</param>
        /// <param name="newUserName">The new user's name details</param>
        public static void PopulateFields(this DirectoryTemplate template, IADUser user, NewUserName newUserName)
        {
            foreach (var fieldValue in template.EffectiveFieldValues)
            {
                try
                {
                    if (fieldValue.Field != null && fieldValue.Value != null)
                        if (fieldValue.Field.FieldName.ToLower() == "homedirectory")
                            user.HomeDirectory = template.ReplaceVariables(fieldValue.Value, newUserName, user.SAMAccountName);
                        else
                            user.NewEntryProperties[fieldValue.Field.FieldName] = template.ReplaceVariables(fieldValue.Value, newUserName, user.SAMAccountName);
                    else if (fieldValue.CustomField != null && fieldValue.Value != null)
                        user.NewEntryProperties[fieldValue.CustomField.FieldName] = template.ReplaceVariables(fieldValue.Value, newUserName, user.SAMAccountName);
                }
                catch (Exception ex)
                {
                    Loggers.ActiveDirectoryLogger.Error("Could not set value for " + fieldValue.Field?.FieldName + ": " + fieldValue.Value?.ToString() + " {@Error}", ex);
                }

            }
        }
        public static string? DnToOu(this string? dN)
        {
            if (dN == null) return null;
            var ouComponents = Regex.Matches(dN, @"OU=([^,]+)")
                            .Select(m => m.Value)
                            .ToList();

            return string.Join(",", ouComponents);
        }

        public static string? ToPrettyOu(this IADOrganizationalUnit? ou)
        {
            if (ou == null) return null;
            var ouComponents = Regex.Matches(ou.DN, @"OU=([^,]*)")
                .Select(m => m.Groups[1].Value)
                .ToList();
            ouComponents.Reverse();
            return "/" + string.Join("/", ouComponents);
        }

        public static string? ParentOU(string? dN)
        {
            return dN != null ? dN.Substring(dN.IndexOf("OU=")) : null;
        }
        /// <summary>
        /// Takes a raw OU DN and removes all OU='s and separates by /'s
        /// </summary>
        /// 
        /// <param name="ou"></param>
        /// <returns></returns>
        public static string? PrettifyOu(string? ou)
        {
            if (ou == null) return null;
            var ouComponents = Regex.Matches(ou, @"OU=([^,]*)")
                .Select(m => m.Groups[1].Value)
                .ToList();
            ouComponents.Reverse();
            return string.Join("/", ouComponents);
        }
      
     
        /// <summary>
        /// Encapsulates a raw DirectoryEntry within a <see cref="IDirectoryEntryAdapter"/>  of the appropriate entry type
        /// </summary>
        /// <param name="r"></param>
        /// <returns>A <see cref="IDirectoryEntryAdapter"/> whose types correspond the directory object type they encapsulate</returns>

        public static IDirectoryEntryAdapter? Encapsulate(this IDirectoryEntry sr, IActiveDirectoryContext context)
        {
            IDirectoryEntryAdapter? thisObject = null;

            if (sr.PropertyContains("objectClass", "top"))
            {
                if (sr.PropertyContains("objectClass","computer"))
                {
                    thisObject = new ADComputer();
                }
                else if (sr.PropertyContains("objectClass", "user"))
                {
                    thisObject = new ADUser();
                }
                else if (sr.PropertyContains("objectClass", "contact"))
                {
                    thisObject = new ADContact();
                }

                else if (sr.PropertyContains("objectClass", "group"))
                {
                    thisObject = new ADGroup();
                }
                else if (sr.PropertyContains("objectClass", "printQueue"))
                {
                    thisObject = new ADPrinter();
                }
                else if (sr.PropertyContains("objectClass", "msFVE-RecoveryInformation"))
                {
                    thisObject = new ADBitLockerRecovery();
                }
                else if (sr.PropertyContains("objectClass", "organizationalUnit") || sr.PropertyContains("objectClass", "container"))
                {
                    thisObject = new ADOrganizationalUnit();
                }
                if (thisObject != null)
                {
                    thisObject.Parse(directory: context, directoryEntry: sr);

                    return thisObject;

                }
                else
                {
                    Loggers.ActiveDirectoryLogger.Warning("Unable to match ad object type. {Object}", sr);

                }
            }
            return null;
        }
        /// <summary>
        /// Extracts the parent Distinguished Name (DN) from a given DN string.
        /// </summary>
        /// <param name="dn">The DN string to parse.</param>
        /// <returns>The parent DN string, or null if no parent exists.</returns>
        /// <example>
        /// <code>
        /// string userDn = "CN=John Doe,CN=Users,DC=example,DC=com";
        /// string parentDn = userDn.GetParentDn();
        /// // parentDn is now "CN=Users,DC=example,DC=com"
        ///
        /// string escapedDn = @"CN=Smith\, John,OU=Accounting,DC=example,DC=com";
        /// string escapedParent = escapedDn.GetParentDn();
        /// // escapedParent is now "OU=Accounting,DC=example,DC=com"
        /// </code>
        /// </example>
        public static string? GetParentDn(this string dn)
        {
            if (string.IsNullOrWhiteSpace(dn))
            {
                return null;
            }

            // This regex uses a negative lookbehind to find the first comma
            // that is not preceded by a backslash. It will split the DN
            // into a maximum of two parts at that first valid delimiter.
            var match = Regex.Match(dn, @"(?<!\\),");

            // If no match is found, there is no parent DN.
            if (!match.Success)
            {
                return null;
            }

            // The parent DN is the substring starting right after the matched comma.
            return dn.Substring(match.Index + 1);
        }
        public static IDirectoryEntry ToIDirectoryEntry(this DirectoryEntry entry)
        {
            return null;
        }
        public static IDirectoryEntry ToIDirectoryEntry(this DirectoryEntry entry, IActiveDirectoryContext directory)
        {
            return new LdapDirectoryEntry(entry.Properties["distinuishedName"].Value?.ToString(),directory);
        }
       


        /// <summary>
        /// Encapsulates a <see cref="System.DirectoryServices.Protocols.SearchResultEntryCollection"/> within a list of <see cref="IDirectoryEntryAdapter"/> of the appropriate entry type.
        /// </summary>
        /// <param name="searchResultEntries">The collection of search result entries from System.DirectoryServices.Protocols.</param>
        /// <param name="context">The Active Directory context.</param>
        /// <returns>A list of <see cref="IDirectoryEntryAdapter"/> whose types correspond to the directory object type they encapsulate.</returns>
        public static List<IDirectoryEntryAdapter> Encapsulate(this System.DirectoryServices.Protocols.SearchResultEntryCollection searchResultEntries, IActiveDirectoryContext context)
        {
            List<IDirectoryEntryAdapter> objects = new List<IDirectoryEntryAdapter>();

            if (searchResultEntries == null || context == null)
            {
                Loggers.ActiveDirectoryLogger.Warning("Encapsulate called with null searchResultEntries or context.");
                return objects;
            }

            try
            {
                foreach (System.DirectoryServices.Protocols.SearchResultEntry sre in searchResultEntries)
                {
                   objects.Add(sre.Encapsulate(context));
                }
            }
            catch (Exception ex)
            {
                Loggers.ActiveDirectoryLogger.Error("Error encapsulating SearchResultEntryCollection: {@Error}", ex);
                // Depending on desired behavior, might clear objects or throw
            }
            return objects;
        }

        private static IDirectoryEntryAdapter? Encapsulate(this SearchResultEntry sre, IActiveDirectoryContext context )
        {
            if (sre == null || sre.Attributes == null) return default;

            IDirectoryEntryAdapter? thisObject = null;
            List<string> objectClasses = new List<string>();

            if (sre.Attributes.Contains("objectClass"))
            {
                foreach (var val in sre.Attributes["objectClass"].GetValues(typeof(byte[])))
                {
                    if (val is byte[] bytes)
                    {
                        objectClasses.Add(Encoding.UTF8.GetString(bytes).ToLowerInvariant());
                    }
                }
            }
            else
            {
                Loggers.ActiveDirectoryLogger.Warning("SearchResultEntry {DN} does not contain objectClass attribute.", sre.DistinguishedName);
                return default; 
            }

            // Determine object type based on objectClass values
            if (objectClasses.Contains("top")) // Basic check
            {
                if (objectClasses.Contains("computer"))
                {
                    thisObject = new ADComputer();
                }
                else if (objectClasses.Contains("user"))
                {
                    thisObject = new ADUser();
                }
                else if (objectClasses.Contains("contact"))
                {
                    thisObject = new ADContact();
                }
                else if (objectClasses.Contains("group"))
                {
                    thisObject = new ADGroup();
                }
                else if (objectClasses.Contains("printqueue")) // Note: printQueue is often lowercase from S.DS.P
                {
                    thisObject = new ADPrinter();
                }
                else if (objectClasses.Contains("msfve-recoveryinformation")) // Note: msFVE-RecoveryInformation is often lowercase
                {
                    thisObject = new ADBitLockerRecovery();
                }
                else if (objectClasses.Contains("organizationalunit") || objectClasses.Contains("container"))
                {
                    thisObject = new ADOrganizationalUnit();
                }
                // Add more types if necessary, e.g. "container" could be a generic DirectoryEntryAdapter if no specific OU logic needed

                if (thisObject != null)
                {
                    // This Parse method signature needs to be created in DirectoryEntryAdapter and its children
                    thisObject.Parse(context, searchResultEntry: sre);
                    return thisObject;
                }
                else
                {
                    Loggers.ActiveDirectoryLogger.Debug("Unrecognized or unhandled object type for DN: {DN}, ObjectClasses: {ObjectClasses}", sre.DistinguishedName, string.Join(", ", objectClasses));
                }
            }
            else
            {
                Loggers.ActiveDirectoryLogger.Debug("Object {DN} does not contain 'top' in objectClass, skipping.", sre.DistinguishedName);
            }
            return default;
        }




        /// <summary>
        /// Extracts the parent distinguished name from a given DN.
        /// </summary>
        /// <param name="dn">The distinguished name.</param>
        /// <returns>The parent DN, or null if no parent exists (e.g., for a domain root) or if the DN is invalid.</returns>
        public static string? GetParentDN(string? dn)
        {
            if (string.IsNullOrEmpty(dn))
            {
                return null;
            }

            int commaIndex = dn.IndexOf(',');
            if (commaIndex > 0 && commaIndex < dn.Length - 1)
            {
                return dn.Substring(commaIndex + 1);
            }
            return null; // No parent DN found (could be a domain root or malformed DN)
        }

        public static string? EscapeLdapSearchFilter(this string? input)
        {
            if (input.IsNullOrEmpty()) return null;
            StringBuilder sb = new();
            foreach (char c in input)
            {
                switch (c)
                {
                    case '\\':
                        sb.Append("\\5c");
                        break;
                    case '*':
                        sb.Append("\\2a");
                        break;
                    case '(':
                        sb.Append("\\28");
                        break;
                    case ')':
                        sb.Append("\\29");
                        break;
                    case '\0': // Null character
                        sb.Append("\\00");
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }


        public static List<ActiveDirectoryFieldOperator> GetOperators(this IActiveDirectoryField field)
        {
            List<ActiveDirectoryFieldOperator> applicableOperators = new List<ActiveDirectoryFieldOperator>();
            if (field == null || field.FieldType==null) return applicableOperators;
            var fieldType = field.FieldType;

            switch (fieldType) {
                case ActiveDirectoryFieldType.Text:
                    applicableOperators.Add(ActiveDirectoryFieldOperator.EqualTo);
                    applicableOperators.Add(ActiveDirectoryFieldOperator.StartsWith);
                    applicableOperators.Add(ActiveDirectoryFieldOperator.EndsWith);
                    applicableOperators.Add(ActiveDirectoryFieldOperator.Contains);
                    break;
                case ActiveDirectoryFieldType.StringList:
                    applicableOperators.Add(ActiveDirectoryFieldOperator.EqualTo);
                    applicableOperators.Add(ActiveDirectoryFieldOperator.Contains);
                    break;
                case ActiveDirectoryFieldType.Date:
                case ActiveDirectoryFieldType.FileTime:
                case ActiveDirectoryFieldType.RawData:
                    applicableOperators.Add(ActiveDirectoryFieldOperator.EqualTo);
                    applicableOperators.Add(ActiveDirectoryFieldOperator.BeforeNow);
                    applicableOperators.Add(ActiveDirectoryFieldOperator.AfterNow);
                    applicableOperators.Add(ActiveDirectoryFieldOperator.HistoricalTimeFrame);
                    applicableOperators.Add(ActiveDirectoryFieldOperator.FutureTimeFrame);
                    break;

            }

            return applicableOperators;

        }

        public static bool IsActionAppropriateForObject(this ActiveDirectoryObjectAction action, ActiveDirectoryObjectType type)
        {

            switch (type)
            {
                case ActiveDirectoryObjectType.User:
                case ActiveDirectoryObjectType.Computer:
                    switch (action)
                    {
                        case ActiveDirectoryObjectAction.Unlock:
                        case ActiveDirectoryObjectAction.Move:
                        case ActiveDirectoryObjectAction.Delete:
                        case ActiveDirectoryObjectAction.Create:
                        case ActiveDirectoryObjectAction.Enable:
                        case ActiveDirectoryObjectAction.Disable:
                        case ActiveDirectoryObjectAction.Rename:
                        case ActiveDirectoryObjectAction.SetPassword:
                            return true;
                        default:
                            return false;
                    }
                case ActiveDirectoryObjectType.Contact:
                    switch (action)
                    {
                        case ActiveDirectoryObjectAction.Move:
                        case ActiveDirectoryObjectAction.Delete:
                        case ActiveDirectoryObjectAction.Create:
                        case ActiveDirectoryObjectAction.Rename:
                            return true;
                        default:
                            return false;
                    }
                case ActiveDirectoryObjectType.Group:
                    switch (action)
                    {
                        case ActiveDirectoryObjectAction.Move:
                        case ActiveDirectoryObjectAction.Delete:
                        case ActiveDirectoryObjectAction.Create:
                        case ActiveDirectoryObjectAction.Unassign:
                        case ActiveDirectoryObjectAction.Assign:
                        case ActiveDirectoryObjectAction.Rename:
                            return true;
                        default:
                            return false;
                    }
                case ActiveDirectoryObjectType.Printer:
                case ActiveDirectoryObjectType.OU:
                    switch (action)
                    {
                        case ActiveDirectoryObjectAction.Move:
                        case ActiveDirectoryObjectAction.Delete:
                        case ActiveDirectoryObjectAction.Create:
                        case ActiveDirectoryObjectAction.Rename:
                            return true;
                        default:
                            return false;
                    }
                case ActiveDirectoryObjectType.BitLocker:
                    switch (action)
                    {
                        case ActiveDirectoryObjectAction.Delete:
                            return true;
                        default:
                            return false;
                    }

                default:
                    return false;
            }
        }

        public static bool IsActionAppropriateForObject(this ObjectAction action, ActiveDirectoryObjectType type) => IsActionAppropriateForObject(action.Action, type);



 
    }


}
