namespace BLAZAM.ActiveDirectory.Interfaces
{
    public interface IADComputerDrive
    {
        double Capacity { get; }
        string? Description { get; }
        bool Dirty { get; }
        DriveType DriveType { get; }
        string? FileSystem { get; }
        double FreeSpace { get; }
        string Letter { get; }
        int MediaType { get; }
        double PercentFree { get; }
        double PercentUsed { get; }
        string? Serial { get; }
        double UsedSpace { get; }
    }
}