namespace BLAZAM.ActiveDirectory.Interfaces
{
    /// <summary>
    /// Represents a BitLocker recovery key stored in Active Directory.
    /// </summary>
    public interface IADBitLockerRecovery : IDirectoryEntryAdapter
    {
        /// <summary>
        /// Gets the unique identifier for the BitLocker recovery key.
        /// </summary>
        Guid? RecoveryId { get; }

        /// <summary>
        /// Gets the BitLocker recovery password used to unlock encrypted drives.
        /// </summary>
        string? RecoveryPassword { get; }
    }
}