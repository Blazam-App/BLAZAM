using BLAZAM.Common.Data.Services;

namespace BLAZAM.ActiveDirectory.Interfaces
{
    /// <summary>
    /// A searcher class for computer objects in an <see cref="IActiveDirectoryContext"/>
    /// </summary>
    public interface IADComputerSearcher
    {
        /// <summary>
        /// Gets the WMI factory for creating WMI connections to remote computers.
        /// </summary>
        WmiFactory WmiFactory { get; }
        
        /// <summary>
        /// Searches for computer objects matching the specified search term.
        /// </summary>
        /// <param name="searchTerm">The search string to match against computer names.</param>
        /// <param name="ignoreDisabled">If true, excludes disabled computer accounts from results. Default is true.</param>
        /// <returns>A list of <see cref="IADComputer"/> objects matching the search criteria.</returns>
        List<IADComputer> FindByString(string searchTerm, bool ignoreDisabled = true);
        
        /// <summary>
        /// Asynchronously searches for computer objects matching the specified search term.
        /// </summary>
        /// <param name="searchTerm">The search string to match against computer names.</param>
        /// <param name="ignoreDisabled">If true, excludes disabled computer accounts from results. Default is true.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of <see cref="IADComputer"/> objects matching the search criteria.</returns>
        Task<List<IADComputer>> FindByStringAsync(string searchTerm, bool ignoreDisabled = true);
        
        /// <summary>
        /// Finds computer objects created within the specified number of days.
        /// </summary>
        /// <param name="maxAgeInDays">The maximum age in days for computers to be considered new. Default is 14 days.</param>
        /// <param name="ignoreDisabledComputers">If true, excludes disabled computer accounts from results. Default is false.</param>
        /// <returns>A list of <see cref="IADComputer"/> objects created within the specified timeframe.</returns>
        List<IADComputer> FindNewComputers(int maxAgeInDays = 14, bool ignoreDisabledComputers = false);
        
        /// <summary>
        /// Asynchronously finds computer objects created within the specified number of days.
        /// </summary>
        /// <param name="maxAgeInDays">The maximum age in days for computers to be considered new. Default is 14 days.</param>
        /// <param name="ignoreDisabledComputers">If true, excludes disabled computer accounts from results. Default is false.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of <see cref="IADComputer"/> objects created within the specified timeframe.</returns>
        Task<List<IADComputer>> FindNewComputersAsync(int maxAgeInDays = 14, bool ignoreDisabledComputers = false);
    }
}