using BLAZAM.ActiveDirectory.Interfaces;

namespace BLAZAM.ActiveDirectory.Adapters
{
    public class ADComputerDrive : IADComputerDrive
    {
        public double FreeSpace { get; internal set; }
        public double UsedSpace { get { return Capacity - FreeSpace; } }
        public double Capacity { get; internal set; }
        public string Letter { get; internal set; }
        public double PercentUsed
        {
            get
            {
                if ((int)Capacity == 0) return 0;
                return UsedSpace / Capacity * 100;
            }
        }
        public double PercentFree
        {
            get
            {
                return FreeSpace / Capacity * 100;
            }
        }

        public string? Description { get; internal set; }
        public string? FileSystem { get; internal set; }
        public bool Dirty { get; internal set; }
        public string? Serial { get; internal set; }
        public int DriveType { get; internal set; }
        public int MediaType { get; internal set; }
    }
}