namespace BLAZAM.ActiveDirectory.Interfaces
{
    /// <summary>
    /// Represents a data drive on a remote computer in Active Directory
    /// </summary>
    public interface IADComputerDrive
    {
        /// <summary>
        /// The capacity of the drive in bytes
        /// </summary>
        double Capacity { get; }

        /// <summary>
        /// The description of the drive
        /// </summary>
        string? Description { get; }


        /// <summary>
        /// Indicated whether the drive needs a disc scan
        /// </summary>
        bool Dirty { get; }

        
        DriveType DriveType { get; }
        string? FileSystem { get; }
        
        /// <summary>
        /// The amount of free space in bytes
        /// </summary>
        double FreeSpace { get; }

        /// <summary>
        /// The assigned drive letter
        /// </summary>
        string Letter { get; }
        int MediaType { get; }

        /// <summary>
        /// The percent of space free
        /// </summary>
        double PercentFree { get; }

        /// <summary>
        /// The percent of space used
        /// </summary>
        double PercentUsed { get; }

        /// <summary>
        /// The serial of the storage device
        /// </summary>
        string? Serial { get; }

        /// <summary>
        /// The amount of used space in bytes
        /// </summary>
        double UsedSpace { get; }
    }
}