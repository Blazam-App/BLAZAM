using BLAZAM.ActiveDirectory.Adapters;

namespace BLAZAM.ActiveDirectory.Interfaces
{
    internal interface IRemoteManagementConnection
    {
        List<IADComputerDrive> Drives { get; }
        ComputerMemory Memory { get; }
        int Processor { get; }
        List<ComputerService> Services { get; }
        List<SharedPrinter> SharePrinters { get; }

        Task<bool> RenameComputerAsync(string newName);
        Task<bool> ShutdownAsync(int delaySeconds = 0, string? message = null, bool force = true, bool reboot = false);
    }
}