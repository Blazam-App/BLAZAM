
namespace BLAZAM.ActiveDirectory.Interfaces
{
    public interface IADBitLockerRecovery : IDirectoryEntryAdapter
    {
        Guid? RecoveryId { get; }
        string? RecoveryPassword { get; }
    }
}