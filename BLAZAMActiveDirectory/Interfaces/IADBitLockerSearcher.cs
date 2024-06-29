using BLAZAM.Common.Data.Services;

namespace BLAZAM.ActiveDirectory.Interfaces
{
    /// <summary>
    /// A searcher class for BitLocker objects in an <see cref="IActiveDirectoryContext"/>
    /// </summary>
    public interface IADBitLockerSearcher
    {
        
        List<IADBitLockerRecovery> FindByRecoveryId(string searchTerm);
        Task<List<IADBitLockerRecovery>> FindByRecoveryIdAsync(string searchTerm);
        List<IADBitLockerRecovery> FindByComputer(IADComputer computer);
        Task<List<IADBitLockerRecovery>> FindByComputerAsync(IADComputer computer);
    }
}