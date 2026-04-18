namespace BLAZAM.ActiveDirectory.Interfaces
{

    /// <summary>
    /// A searcher class for printer objects in an <see cref="IActiveDirectoryContext"/>
    /// </summary>
    public interface IADPrinterSearcher
    {
        /// <summary>
        /// Finds printers that have been modified within a specified time period
        /// </summary>
        /// <param name="ignoreDisabledPrinters">If true, excludes disabled printers from results. Default is true.</param>
        /// <param name="daysBackToSearch">Number of days to search back for changes. Default is 90 days.</param>
        /// <returns>A list of printers that have been modified within the specified time period</returns>
        List<IADPrinter> FindChangedPrinters(bool? ignoreDisabledPrinters = true, int daysBackToSearch = 90);
        
        /// <summary>
        /// Asynchronously finds printers that have been modified within a specified time period
        /// </summary>
        /// <param name="ignoreDisabledPrinters">If true, excludes disabled printers from results. Default is true.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of modified printers.</returns>
        Task<List<IADPrinter>> FindChangedPrintersAsync(bool? ignoreDisabledPrinters = true);
        
        /// <summary>
        /// Finds printers that were created within a specified number of days
        /// </summary>
        /// <param name="maxAgeInDays">Maximum age in days for printers to be considered new. Default is 14 days.</param>
        /// <param name="ignoreDisabledPrinters">If true, excludes disabled printers from results. Default is true.</param>
        /// <returns>A list of newly created printers within the specified age</returns>
        List<IADPrinter> FindNewPrinters(int maxAgeInDays = 14, bool? ignoreDisabledPrinters = true);
        
        /// <summary>
        /// Asynchronously finds printers that were created within a specified number of days
        /// </summary>
        /// <param name="maxAgeInDays">Maximum age in days for printers to be considered new. Default is 14 days.</param>
        /// <param name="ignoreDisabledPrinters">If true, excludes disabled printers from results. Default is true.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of newly created printers.</returns>
        Task<List<IADPrinter>> FindNewPrintersAsync(int maxAgeInDays = 14, bool? ignoreDisabledPrinters = true);
        
        /// <summary>
        /// Finds a single printer by its name
        /// </summary>
        /// <param name="searchTerm">The printer name to search for</param>
        /// <param name="ignoreDisabledPrinters">If true, excludes disabled printers from results. Default is true.</param>
        /// <returns>The matching printer, or null if not found</returns>
        IADPrinter? FindPrinterByName(string? searchTerm, bool? ignoreDisabledPrinters = true);
        
        /// <summary>
        /// Finds a printer by its container name
        /// </summary>
        /// <param name="searchTerm">The container name to search for</param>
        /// <param name="ignoreDisabledPrinters">If true, excludes disabled printers from results. Default is true.</param>
        /// <param name="exactMatch">If true, requires an exact match of the container name. Default is false.</param>
        /// <returns>The matching printer, or null if not found</returns>
        IADPrinter? FindPrintersByContainerName(string? searchTerm, bool? ignoreDisabledPrinters = true, bool exactMatch = false);
        
        /// <summary>
        /// Finds printers matching a search string
        /// </summary>
        /// <param name="searchTerm">The search term to match against printer properties</param>
        /// <param name="ignoreDisabledPrinters">If true, excludes disabled printers from results. Default is true.</param>
        /// <param name="exactMatch">If true, requires an exact match of the search term. Default is false.</param>
        /// <returns>A list of printers matching the search criteria</returns>
        List<IADPrinter> FindPrintersByString(string? searchTerm, bool? ignoreDisabledPrinters = true, bool exactMatch = false);
        
        /// <summary>
        /// Asynchronously finds printers matching a search string
        /// </summary>
        /// <param name="searchTerm">The search term to match against printer properties</param>
        /// <param name="ignoreDisabledPrinters">If true, excludes disabled printers from results. Default is true.</param>
        /// <param name="exactMatch">If true, requires an exact match of the search term. Default is false.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of matching printers.</returns>
        Task<List<IADPrinter>> FindPrintersByStringAsync(string? searchTerm, bool? ignoreDisabledPrinters = true, bool exactMatch = false);
    }
}