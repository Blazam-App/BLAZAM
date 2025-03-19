using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Helpers;
using BLAZAM.Logger;
using System.Management;
using System.Runtime.InteropServices;

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
                ConnectionOptions connectionOptions = new();
                connectionOptions.Username = settings.Username + "@" + settings.FQDN;
                connectionOptions.SecurePassword = settings.Password.Decrypt().ToSecureString();
                connectionOptions.Impersonation = ImpersonationLevel.Impersonate;
                connectionOptions.Timeout = TimeSpan.FromSeconds(5);
                connectionOptions.Authentication = AuthenticationLevel.PacketPrivacy;

                ManagementScope managementScope = new(string.Format("\\\\{0}\\root\\cimv2", hostName), connectionOptions);
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

            throw new WmiConnectionException();
        }

        public IActiveDirectoryContext Directory { get; }
    }
}