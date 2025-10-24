using BLAZAM.Global.Enums;

namespace BLAZAM.Services.Background
{
    public interface IConnectionMonitor
    {
        ServiceConnectionState Status { get; }
        AppDelegate<ServiceConnectionState>? OnConnectedChanged { get; set; }

        void Monitor();
    }
}
