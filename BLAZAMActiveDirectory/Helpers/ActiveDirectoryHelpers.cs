using BLAZAM.ActiveDirectory;
using BLAZAM.ActiveDirectory.Adapters;
using BLAZAM.ActiveDirectory.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Reflection;
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


        public static Process? Shadow(this IRemoteSession session, bool withoutPermission = false)
        {
            if (session == null) return null;
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
                            thisObject.Parse(sr, ActiveDirectoryContext.Instance);
                        }
                        else if (sr.Properties["objectClass"].Contains("user"))
                        {
                            thisObject = new ADUser();
                            thisObject.Parse(sr, ActiveDirectoryContext.Instance);
                        }
                        else if (sr.Properties["objectClass"].Contains("organizationalUnit"))
                        {
                            thisObject = new ADOrganizationalUnit();
                            thisObject.Parse(sr, ActiveDirectoryContext.Instance);
                        }
                        else if (sr.Properties["objectClass"].Contains("group"))
                        {
                            thisObject = new ADGroup();
                            thisObject.Parse(sr, ActiveDirectoryContext.Instance);
                        }
                        else if (sr.Properties["objectClass"].Contains("printQueue"))
                        {
                            thisObject = new ADPrinter();
                            thisObject.Parse(sr, ActiveDirectoryContext.Instance);
                        }
                        if (thisObject != null)
                            objects.Add(thisObject);
                    }
                    thisObject = null;

                }
            }
            return objects;
        }
        public static List<IDirectoryEntryAdapter> Encapsulate(this DirectoryEntries r)
        {
            List<IDirectoryEntryAdapter> objects = new();


            if (r != null)
            {

                IDirectoryEntryAdapter? thisObject = null;
                foreach (DirectoryEntry sr in r)
                {
                    if (sr.Properties["objectClass"].Contains("top"))
                    {
                        if (sr.Properties["objectClass"].Contains("computer"))
                        {
                            thisObject = new ADComputer();
                            thisObject.Parse(sr, ActiveDirectoryContext.Instance);
                        }
                        else if (sr.Properties["objectClass"].Contains("user"))
                        {
                            thisObject = new ADUser();
                            thisObject.Parse(sr, ActiveDirectoryContext.Instance);
                        }
                        else if (sr.Properties["objectClass"].Contains("organizationalUnit"))
                        {
                            thisObject = new ADOrganizationalUnit();
                            thisObject.Parse(sr, ActiveDirectoryContext.Instance);
                        }
                        else if (sr.Properties["objectClass"].Contains("group"))
                        {
                            thisObject = new ADGroup();
                            thisObject.Parse(sr, ActiveDirectoryContext.Instance);
                        }
                        else if (sr.Properties["objectClass"].Contains("printQueue"))
                        {
                            thisObject = new ADPrinter();
                            thisObject.Parse(sr, ActiveDirectoryContext.Instance);
                        }
                        if (thisObject != null)
                            objects.Add(thisObject);
                    }
                    thisObject = null;

                }
            }
            return objects;
        }
    }
}
