using BLAZAM.Common.Data.Services;

namespace BLAZAM.Common.Data.ActiveDirectory.Interfaces
{
    public interface IADComputerSearcher
    {
        WmiFactoryService WmiFactory { get; }
        List<IADComputer> FindByString(string searchTerm, bool ignoreDisable);
        Task<List<IADComputer>> FindByStringAsync(string searchTerm, bool ignoreDisabled = true);
    }
}