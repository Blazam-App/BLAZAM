using BLAZAM.ActiveDirectory.Adapters;
using System.Net;

namespace BLAZAM.ActiveDirectory.Interfaces
{
    /// <summary>
    /// Represents a computer object in Active Directory.
    /// </summary>
    /// <remarks>
    /// For realtime monitoring, remember to call <see cref="MonitorOnlineStatus(int)"/> after instantiation.
    /// </remarks>
    public interface IADComputer : IAccountDirectoryAdapter
    {
        /// <summary>
        /// This computer's Operating System
        /// </summary>
        string? OperatingSystem { get; set; }
        /// <summary>
        /// Indiates whether this computer is reachable by the server. 
        /// Null indicates that the check has not yet completed.
        /// </summary>
        bool? IsOnline { get; }
        /// <summary>
        /// If this computer is online, this is the resolved IP address from the server.
        /// Otherwise, this is null
        /// </summary>
        IPHostEntry? IPHostEntry { get; set; }

        /// <summary>
        /// Called when this computers <see cref="IsOnline"/> status changes
        /// </summary>
        AppEvent<bool> OnOnlineChanged { get; set; }
        /// <summary>
        /// The realtime CPU usage percentage
        /// </summary>
        int Processor { get; }
        /// <summary>
        /// The realtime RAM usage percentage
        /// </summary>
        double MemoryUsedPercent { get; }

        /// <summary>
        /// All services installed on this computer
        /// </summary>
        List<ComputerService> Services { get; }

        /// <summary>
        /// All shared printers on this computer
        /// </summary>
        List<SharedPrinter> SharedPrinters { get; }

        Task<List<IADBitLockerRecovery>?> GetBitLockerRecoveryAsync();

        /// <summary>
        /// Gets the drive details from this computer
        /// </summary>
        /// <returns>The drive details</returns>
        List<IADComputerDrive> GetDrives();
        /// <summary>
        /// Gets the drive details from this computer asynchronously
        /// </summary>
        /// <returns>The drive details</returns>
        Task<List<IADComputerDrive>> GetDrivesAsync();

        /// <summary>
        /// Gets all connected sessions on this computer asynchronously
        /// </summary>
        /// <returns>A list of logged in sessions</returns>
        Task<List<IRemoteSession>> GetRemoteSessionsAsync();

        /// <summary>
        /// A trigger to start monitoring the online status of this computer
        /// </summary>
        /// <remarks>
        /// No checks are performed before calling this method
        /// </remarks>
        /// <param name="timeout"></param>
        void MonitorOnlineStatus(int timeout = 500);
    }
}