using BLAZAM.Common.Data;

namespace BLAZAM.Services.Background
{
    public interface IConnectionMonitor
    {
        public ServiceConnectionState Status { get; }
        public AppEvent<ServiceConnectionState>? OnConnectedChanged { get; set; }

        public void Monitor();
    }
}
