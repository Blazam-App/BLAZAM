using BLAZAM.Common.Data.Services;

namespace BLAZAM.ActiveDirectory.Interfaces
{
    /// <summary>
    /// A searcher class for computer objects in an <see cref="IActiveDirectoryContext"/>
    /// </summary>
    public interface IADComputerSearcher
    {
        WmiFactory WmiFactory { get; }
        List<IADComputer> FindByString(string searchTerm, bool ignoreDisable);
        Task<List<IADComputer>> FindByStringAsync(string searchTerm, bool ignoreDisabled = true);
        List<IADComputer> FindNewComputers(int maxAgeInDays = 14, bool ignoreDisabledComputers = false);
        Task<List<IADComputer>> FindNewComputersAsync(int maxAgeInDays = 14, bool ignoreDisabledComputers = false);
    }
}