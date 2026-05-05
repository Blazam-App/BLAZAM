using Cassia;
using System.Net;
using System.Security.Principal;

namespace BLAZAM.ActiveDirectory.Interfaces
{
    /// <summary>
    /// Represents a remote terminal server session with monitoring and management capabilities.
    /// </summary>
    public interface IRemoteSession : IDisposable
    {
        /// <summary>
        /// Gets the IP address of the client connected to this session.
        /// </summary>
        IPAddress ClientIPAddress { get; }

        /// <summary>
        /// Gets the current connection state of the session.
        /// </summary>
        Cassia.ConnectionState ConnectionState { get; }

        /// <summary>
        /// Gets the date and time when the session was connected, or null if not connected.
        /// </summary>
        DateTime? ConnectTime { get; }

        /// <summary>
        /// Gets the amount of time the session has been idle, or null if not applicable.
        /// </summary>
        TimeSpan? IdleTime { get; }

        /// <summary>
        /// Gets the date and time when the user logged into the session, or null if not logged in.
        /// </summary>
        DateTime? LoginTime { get; }

        /// <summary>
        /// Gets a value indicating whether this session is currently being monitored.
        /// </summary>
        bool Monitoring { get; }

        /// <summary>
        /// Gets the terminal server that hosts this session, or null if not available.
        /// </summary>
        ITerminalServer? Server { get; }

        /// <summary>
        /// Gets the unique identifier for this session on the terminal server.
        /// </summary>
        int SessionId { get; }

        /// <summary>
        /// Gets the Windows account of the user associated with this session.
        /// </summary>
        NTAccount User { get; }

        /// <summary>
        /// Gets or sets the delegate to be invoked when the session goes down or disconnects.
        /// </summary>
        AppDelegate<IRemoteSession> OnSessionDown { get; set; }

        /// <summary>
        /// Gets or sets the delegate to be invoked when the session state is updated.
        /// </summary>
        AppDelegate<IRemoteSession> OnSessionUpdated { get; set; }

        /// <summary>
        /// Disconnects the remote session without logging off the user.
        /// </summary>
        /// <param name="synchronous">If true, waits for the operation to complete before returning; otherwise, performs the operation asynchronously.</param>
        void Disconnect(bool synchronous = false);

        /// <summary>
        /// Determines whether the specified object is equal to the current session.
        /// </summary>
        /// <param name="obj">The object to compare with the current session.</param>
        /// <returns>true if the specified object is equal to the current session; otherwise, false.</returns>
        bool Equals(object? obj);

        /// <summary>
        /// Returns the hash code for this session.
        /// </summary>
        /// <returns>A hash code for the current session.</returns>
        int GetHashCode();

        /// <summary>
        /// Logs off the user and ends the remote session.
        /// </summary>
        /// <param name="synchronous">If true, waits for the operation to complete before returning; otherwise, performs the operation asynchronously.</param>
        void Logoff(bool synchronous = false);

        /// <summary>
        /// Sends a message to the user connected to this session.
        /// </summary>
        /// <param name="message">The message text to send to the user.</param>
        void SendMessage(string message);
    }
}