using BLAZAM.ActiveDirectory.Adapters;
using System.Net;

namespace BLAZAM.ActiveDirectory.Interfaces
{
    public interface IADComputer : IAccountDirectoryAdapter
    {
        string? OperatingSystem { get; set; }
        bool? IsOnline { get; }
        IPHostEntry? IPHostEntry { get; set; }
        AppEvent<bool> OnOnlineChanged { get; set; }
        int Processor { get; }
        double MemoryUsedPercent { get; }
        List<ComputerService> Services { get; }
        List<SharedPrinter> SharedPrinters { get; }
        List<IADComputerDrive> GetDrives();
        Task<List<IADComputerDrive>> GetDrivesAsync();
        Task<List<IRemoteSession>> GetRemoteSessionsAsync();
        void MonitorOnlineStatus(int timeout = 500);
    }
}