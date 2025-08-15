using BLAZAM.Global.Enums;

namespace BLAZAM.Services.Background
{
    public interface IConnectionMonitor
    {
        public ServiceConnectionState Status { get; }
        public AppDelegate<ServiceConnectionState>? OnConnectedChanged { get; set; }

        public void Monitor();
    }
}
