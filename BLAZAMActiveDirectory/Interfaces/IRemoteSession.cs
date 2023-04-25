using Cassia;
using System.Net;
using System.Security.Principal;

namespace BLAZAM.ActiveDirectory.Interfaces
{
    public interface IRemoteSession
    {
        IPAddress ClientIPAddress { get; }
        Cassia.ConnectionState ConnectionState { get; }
        DateTime? ConnectTime { get; }
        TimeSpan? IdleTime { get; }
        DateTime? LoginTime { get; }
        bool Monitoring { get; }
        ITerminalServer Server { get; }
        int SessionId { get; }
        NTAccount User { get; }
        AppEvent<IRemoteSession> OnSessionDown { get; set; }
        AppEvent<IRemoteSession> OnSessionUpdated { get; set; }

        void Disconnect(bool synchronous = false);
        bool Equals(object? obj);
        int GetHashCode();
        void Logoff(bool synchronous = false);
        void SendMessage(string message);
    }
}