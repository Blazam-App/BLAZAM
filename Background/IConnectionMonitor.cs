namespace BLAZAM.Server.Background
{
    public interface IConnectionMonitor
    {
        public ConnectionState Connected { get; }
        public AppEvent<ConnectionState>? OnConnectedChanged { get; set; }

        public void Monitor();
    }
}
