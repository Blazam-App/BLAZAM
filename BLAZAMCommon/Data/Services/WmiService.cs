using System.Collections.Generic;
using System.Drawing.Printing;
using System.Management;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using BLAZAM.Common.Data.ActiveDirectory.Models;
using BLAZAM.Common.Data.Database;
using Microsoft.EntityFrameworkCore;

namespace BLAZAM.Common.Data.Services
{
    public class WmiFactoryService
    {

        public WmiFactoryService(IDbContextFactory<DatabaseContext> factory)
        {
            Factory = factory;
        }

        public ManagementScope CreateWmiConnection(string hostName)
        {
            using (var context = Factory.CreateDbContext())
            {
                var settings = context.ActiveDirectorySettings.FirstOrDefault();
                if (settings != null) {
                    SecureString securePassword = new NetworkCredential("", settings.Password).SecurePassword;
                    ConnectionOptions connectionOptions = new ConnectionOptions();
                    connectionOptions.Username = settings.Username+"@"+settings.FQDN;
                   // connectionOptions.Password = settings.Password;
                    connectionOptions.SecurePassword = securePassword;
                    connectionOptions.Impersonation = ImpersonationLevel.Impersonate;
                    connectionOptions.Timeout = TimeSpan.FromSeconds(5);
                    
                    ManagementScope managementScope = new ManagementScope(string.Format("\\\\{0}\\root\\cimv2", hostName), connectionOptions);
                    try
                    {
                        managementScope.Connect();
                    }catch(UnauthorizedAccessException ex)
                    {
                        Loggers.ActiveDirectryLogger.Error("Unauthorized access exception connecting wmi to " + hostName, ex);
                    }
                    catch(COMException ex)
                    {
                        Loggers.ActiveDirectryLogger.Error("COM Exception while connecting to WMI on " + hostName, ex);
                    }
                    return managementScope; 
                }
            }
            throw new WmiConnectionFailure();
        }

        public IDbContextFactory<DatabaseContext> Factory { get; }
    }

    [Serializable]
    internal class WmiConnectionFailure : Exception
    {
        public WmiConnectionFailure()
        {
        }

        public WmiConnectionFailure(string? message) : base(message)
        {
        }

        public WmiConnectionFailure(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected WmiConnectionFailure(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}