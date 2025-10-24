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
                ConnectionOptions connectionOptions = new()
                {
                    Username = settings.Username + "@" + settings.FQDN,
                    SecurePassword = settings.Password.Decrypt().ToSecureString(),
                    Impersonation = ImpersonationLevel.Impersonate,
                    Timeout = TimeSpan.FromSeconds(5),
                    Authentication = AuthenticationLevel.PacketPrivacy
                };

                ManagementScope managementScope = new(string.Format("\\\\{0}\\root\\cimv2", hostName), connectionOptions);
                try
                {
                    managementScope.Connect();
                }
                catch (UnauthorizedAccessException ex)
                {
                    Loggers.ActiveDirectoryLogger.Warning(ex, "Unauthorized access exception connecting wmi to {@Hostname}", hostName);
                }
                catch (COMException ex)
                {
                    Loggers.ActiveDirectoryLogger.Warning(ex, "COM Exception while connecting to WMI on  {@Hostname}", hostName);
                }
                catch (Exception ex)
                {
                    Loggers.ActiveDirectoryLogger.Error(ex, "Error connecting to WMI {@Hostname}", hostName);

                }
                return managementScope;
            }

            throw new WmiConnectionException();
        }

        public IActiveDirectoryContext Directory { get; }
    }
}