using BLAZAM.ActiveDirectory;
using BLAZAM.ActiveDirectory.Adapters;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using BLAZAM.Logger;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BLAZAM.Helpers
{
    public static class ActiveDirectoryHelpers
    {

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
        public static IEnumerable<IDirectoryEntryAdapter> MoveToTop(this IEnumerable<IDirectoryEntryAdapter> enumerable, Func<IDirectoryEntryAdapter,bool>matchingPredicate)
        {
            var list = enumerable.ToList();
            if (list.Count() < 1) return list;
            List<IDirectoryEntryAdapter> mathingItems=new List<IDirectoryEntryAdapter>();
            for (int x =0; x < list.Count(); x++)
            {

                if (matchingPredicate.Invoke(list[x]))
                {
                    var toMove = list[x];
                    list.RemoveAt(x);
                    x--;
                    mathingItems.Add(toMove);
                    
                }
            }
            if(mathingItems.Count > 0)
            {
                list.InsertRange(0,mathingItems.OrderBy(x=>x.CanonicalName));
            }
            return list.AsEnumerable();
            return default;
        }

        public static Process? Shadow(this IRemoteSession session, bool withoutPermission = false)
        {
            if (session == null || session.Server==null) return null;
            string command = "mstsc.exe";
            string arguments = "/v:" + session.Server.ServerName + " /shadow:" + session.SessionId;
            if (withoutPermission) arguments += " /noConsentPrompt";
            return Process.Start(command, arguments);
            //Debug.TrackEvent("Shadow (Consent)", properties);
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
            return dN!=null?dN.Substring(dN.IndexOf("OU=")):null;
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
        public static List<IDirectoryEntryAdapter> Encapsulate(this SearchResultCollection r)
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
                        else if (sr.Properties["objectClass"].Contains("organizationalUnit"))
                        {
                            thisObject = new ADOrganizationalUnit();
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
                        if (thisObject != null)
                        {
                            thisObject.Parse(directory: ActiveDirectoryContext.Instance, searchResult: sr);


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

        public static IDirectoryEntryAdapter? Encapsulate(this DirectoryEntry sr)
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
                else if (sr.Properties["objectClass"].Contains("organizationalUnit"))
                {
                    thisObject = new ADOrganizationalUnit();
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
                if (thisObject != null)
                {
                    thisObject.Parse(directory: ActiveDirectoryContext.Instance, directoryEntry: sr);

                    return thisObject;

                }
                else
                {
                    Loggers.ActiveDirectryLogger.Warning("Unable to match ad object type. {Object}", sr);
                    
                }
            }
            return null;
        }
            /// <summary>
            /// Encapsulates a raw DirectoryEntry search's <see cref="DirectoryEntries"/> within a <see cref="IDirectoryEntryAdapter"/>  of the appropriate entry type
            /// </summary>
            /// <remarks>
            /// This is used when getting child ojects from a OU
            /// </remarks>
            /// <param name="r"></param>
            /// <returns>A list of <see cref="IDirectoryEntryAdapter"/> whose types correspond the directory object type they encapsulate</returns>
            public static List<IDirectoryEntryAdapter> Encapsulate(this DirectoryEntries r)
        {
            List<IDirectoryEntryAdapter> objects = new();


            if (r != null)
            {

                foreach (DirectoryEntry sr in r)
                {
                   var encapsulated = Encapsulate(sr);
                    if(encapsulated != null)
                         objects.Add(encapsulated);

                }
            }
            return objects;
        }
    }
}
