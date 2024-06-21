
namespace BLAZAM.ActiveDirectory.Interfaces
{
    public interface IADBitLockerRecovery
    {
        Guid? RecoveryId { get;  }
        string? RecoveryPassword { get; }
    }
}