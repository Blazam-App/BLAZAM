using BLAZAM.Common.Data;
using BLAZAM.Common.Data.ActiveDirectory;
using BLAZAM.Common.Data.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data.Database;
using BLAZAM.Server.Pages.Error;
using Microsoft.EntityFrameworkCore;

namespace BLAZAM.Server.Background
{
    public class DirectoryMonitor : ConnectionMonitor
    {
        private IActiveDirectoryContext _directry;

        public DirectoryMonitor( IActiveDirectoryContext directry)
        {

            _directry = directry;
            _directry.OnStatusChanged += StatusChanged;
        }

        private void StatusChanged(DirectoryConnectionStatus value)
        {
            switch (value)
            {
                case DirectoryConnectionStatus.OK:
                    Status = ServiceConnectionState.Up;

                    break;
                case DirectoryConnectionStatus.BadCredentials:
                    Oops.ErrorMessage = "Bad Credentials!";
                    goto default; 
                case DirectoryConnectionStatus.BadConfiguration:
                    Oops.ErrorMessage = "Bad Active Directory Configuration!";
                    goto default;
                case DirectoryConnectionStatus.EncryptionError:
                    Oops.ErrorMessage = "Encryption Error";
                    goto default;
                case DirectoryConnectionStatus.ConnectionDown:
                    Oops.ErrorMessage = "Active Directory server connection is down.";
                    goto default;

                case DirectoryConnectionStatus.ServerDown:
                    Oops.ErrorMessage = "Directory Server appears down";
                    goto default;
                case DirectoryConnectionStatus.UnreachableConfiguration:
                    Oops.ErrorMessage = "Database is corrupt, or installation was incomplete!";
                    goto default;
                case DirectoryConnectionStatus.Connecting:
                    Status = ServiceConnectionState.Connecting;
                    break;
                default:

                    Status = ServiceConnectionState.Down;

                    break;
            }
        }

        protected override void Tick(object? state)
        {
            StatusChanged(_directry.Status);

        }
    }
}