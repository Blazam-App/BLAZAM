using BLAZAM.ActiveDirectory;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Global.Enums;

namespace BLAZAM.Services.Background
{
    public class DirectoryMonitor : ConnectionMonitor
    {
        private IActiveDirectoryContext _directory;

        public DirectoryMonitor(IActiveDirectoryContext directry)
        {

            _directory = directry;
            _directory.OnStatusChanged += StatusChanged;
        }

        private void StatusChanged(DirectoryConnectionStatus value)
        {
            switch (value)
            {
                case DirectoryConnectionStatus.OK:
                    Status = ServiceConnectionState.Up;
                    break;
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
            StatusChanged(_directory.Status);

        }
    }
}