using BLAZAM.ActiveDirectory.Exceptions;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data.Database;
using BLAZAM.Database.Context;
using BLAZAM.Helpers;
using BLAZAM.Logger;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Management;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;

namespace BLAZAM.Common.Data.Services
{
    public class WmiFactory
    {

        public WmiFactory(IActiveDirectoryContext directory)
        {
            Directory = directory;
        }

        public ManagementScope CreateWmiConnection(string hostName)
        {

            var settings = Directory.ConnectionSettings;
            if (settings != null)
            {
                ConnectionOptions connectionOptions = new ConnectionOptions();
                connectionOptions.Username = settings.Username + "@" + settings.FQDN;
                connectionOptions.SecurePassword = settings.Password.Decrypt().ToSecureString();
                connectionOptions.Impersonation = ImpersonationLevel.Impersonate;
                connectionOptions.Timeout = TimeSpan.FromSeconds(5);

                ManagementScope managementScope = new ManagementScope(string.Format("\\\\{0}\\root\\cimv2", hostName), connectionOptions);
                try
                {
                    managementScope.Connect();
                }
                catch (UnauthorizedAccessException ex)
                {
                    Loggers.ActiveDirectoryLogger.Warning("Unauthorized access exception connecting wmi to " + hostName + " {@Error}", ex);
                }
                catch (COMException ex)
                {
                    Loggers.ActiveDirectoryLogger.Warning("COM Exception while connecting to WMI on " + hostName + " {@Error}", ex);
                }
                catch (Exception ex)
                {
                    Loggers.ActiveDirectoryLogger.Error("Error connecting to WMI " + hostName + " {@Error}", ex);

                }
                return managementScope;
            }

            throw new WmiConnectionFailure();
        }

        public IActiveDirectoryContext Directory { get; }
    }
}