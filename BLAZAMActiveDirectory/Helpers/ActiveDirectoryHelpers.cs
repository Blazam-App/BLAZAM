using BLAZAM.ActiveDirectory;
using BLAZAM.ActiveDirectory.Adapters;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using BLAZAM.Database.Models;
using BLAZAM.Database.Models.Templates;
using BLAZAM.Logger;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
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
                            user.HomeDirectory = template.ReplaceVariables(fieldValue.Value, newUserName, user.SamAccountName);
                        else
                            user.NewEntryProperties[fieldValue.Field.FieldName] = template.ReplaceVariables(fieldValue.Value, newUserName, user.SamAccountName);
                    else if (fieldValue.CustomField != null && fieldValue.Value != null)
                        user.NewEntryProperties[fieldValue.CustomField.FieldName] = template.ReplaceVariables(fieldValue.Value, newUserName, user.SamAccountName);
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
        /// Encapsulates a raw DirectoryEntry search's <see cref="SearchResultCollection"/> within a <see cref="IDirectoryEntryAdapter"/>  of the appropriate entry type
        /// </summary>
        /// <param name="r"></param>
        /// <returns>A list of <see cref="IDirectoryEntryAdapter"/> whose types correspond the directory object type they encapsulate</returns>
        public static List<IDirectoryEntryAdapter> Encapsulate(this SearchResultCollection r, IActiveDirectoryContext context)
        {
            List<IDirectoryEntryAdapter> objects = new();


            if (r != null && r.Count > 0)
            {

                IDirectoryEntryAdapter? thisObject = null;
                foreach (SearchResult sr in r)
                {
                    if (sr.Properties["objectClass"].Contains("top"))
                    {
                        if (sr.Properties["objectClass"].Contains("computer"))
                        {
                            thisObject = new ADComputer();
                        }
                        else if (sr.Properties["objectClass"].Contains("user"))
                        {
                            thisObject = new ADUser();
                        }

                        else if (sr.Properties["objectClass"].Contains("group"))
                        {
                            thisObject = new ADGroup();
                        }
                        else if (sr.Properties["objectClass"].Contains("printQueue"))
                        {
                            thisObject = new ADPrinter();
                        }
                        else if (sr.Properties["objectClass"].Contains("msFVE-RecoveryInformation"))
                        {
                            thisObject = new ADBitLockerRecovery();
                        }
                        else if (sr.Properties["objectClass"].Contains("organizationalUnit") || sr.Properties["objectClass"].Contains("container"))
                        {
                            thisObject = new ADOrganizationalUnit();
                        }
                        if (thisObject != null)
                        {
                            thisObject.Parse(directory: context, searchResult: sr);


                            objects.Add(thisObject);

                        }
                    }
                    thisObject = null;

                }
            }
            return objects;
        }
        /// <summary>
        /// Encapsulates a raw DirectoryEntry within a <see cref="IDirectoryEntryAdapter"/>  of the appropriate entry type
        /// </summary>
        /// <param name="r"></param>
        /// <returns>A <see cref="IDirectoryEntryAdapter"/> whose types correspond the directory object type they encapsulate</returns>

        public static IDirectoryEntryAdapter? Encapsulate(this DirectoryEntry sr, IActiveDirectoryContext context)
        {
            IDirectoryEntryAdapter? thisObject = null;

            if (sr.Properties["objectClass"].Contains("top"))
            {
                if (sr.Properties["objectClass"].Contains("computer"))
                {
                    thisObject = new ADComputer();
                }
                else if (sr.Properties["objectClass"].Contains("user"))
                {
                    thisObject = new ADUser();
                }

                else if (sr.Properties["objectClass"].Contains("group"))
                {
                    thisObject = new ADGroup();
                }
                else if (sr.Properties["objectClass"].Contains("printQueue"))
                {
                    thisObject = new ADPrinter();
                }
                else if (sr.Properties["objectClass"].Contains("msFVE-RecoveryInformation"))
                {
                    thisObject = new ADBitLockerRecovery();
                }
                else if (sr.Properties["objectClass"].Contains("organizationalUnit") || sr.Properties["objectClass"].Contains("container"))
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
        /// Encapsulates a raw DirectoryEntry search's <see cref="DirectoryEntries"/> within a <see cref="IDirectoryEntryAdapter"/>  of the appropriate entry type
        /// </summary>
        /// <remarks>
        /// This is used when getting child objects from a OU
        /// </remarks>
        /// <param name="r"></param>
        /// <returns>A list of <see cref="IDirectoryEntryAdapter"/> whose types correspond the directory object type they encapsulate</returns>
        public static List<IDirectoryEntryAdapter> Encapsulate(this DirectoryEntries r, IActiveDirectoryContext context)
        {
            List<IDirectoryEntryAdapter> objects = new();


            if (r != null)
            {

                foreach (DirectoryEntry sr in r)
                {
                    var encapsulated = Encapsulate(sr, context);
                    if (encapsulated != null)
                        objects.Add(encapsulated);

                }
            }
            return objects;
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
            var fieldType = field.FieldType;
            List<ActiveDirectoryFieldOperator> applicableOperators = new List<ActiveDirectoryFieldOperator>();

            switch (fieldType) {
                case ActiveDirectoryFieldType.Text:
                    applicableOperators.Add(ActiveDirectoryFieldOperator.Equals);
                    applicableOperators.Add(ActiveDirectoryFieldOperator.StartsWith);
                    applicableOperators.Add(ActiveDirectoryFieldOperator.EndsWith);
                    applicableOperators.Add(ActiveDirectoryFieldOperator.Contains);
                    break;
                case ActiveDirectoryFieldType.StringList:
                    applicableOperators.Add(ActiveDirectoryFieldOperator.Equals);
                    applicableOperators.Add(ActiveDirectoryFieldOperator.Contains);
                    break;
                case ActiveDirectoryFieldType.Date:
                case ActiveDirectoryFieldType.RawData:
                    applicableOperators.Add(ActiveDirectoryFieldOperator.Equals);
                    applicableOperators.Add(ActiveDirectoryFieldOperator.BeforeNow);
                    applicableOperators.Add(ActiveDirectoryFieldOperator.AfterNow);
                    applicableOperators.Add(ActiveDirectoryFieldOperator.HistoricalTimeFrame);
                    applicableOperators.Add(ActiveDirectoryFieldOperator.FutureTimeFrame);
                    break;

            }
                  
            return applicableOperators;

        }
    }
}
