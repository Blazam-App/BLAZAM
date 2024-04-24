namespace BLAZAM.ActiveDirectory.Interfaces
{

    /// <summary>
    /// A searcher class for printer objects in an <see cref="IActiveDirectoryContext"/>
    /// </summary>
    public interface IADPrinterSearcher
    {
        List<IADPrinter> FindChangedPrinters(bool? ignoreDisabledPrinters = true, int daysBackToSearch = 90);
        Task<List<IADPrinter>> FindChangedPrintersAsync(bool? ignoreDisabledPrinters = true);
        List<IADPrinter> FindNewPrinters(int maxAgeInDays = 14, bool? ignoreDisabledPrinters = true);
        Task<List<IADPrinter>> FindNewPrintersAsync(int maxAgeInDays = 14, bool? ignoreDisabledPrinters = true);
        IADPrinter? FindPrinterByName(string? searchTerm, bool? ignoreDisabledPrinters = true);
        IADPrinter? FindPrintersByContainerName(string? searchTerm, bool? ignoreDisabledPrinters = true, bool exactMatch = false);
        List<IADPrinter> FindPrintersByString(string? searchTerm, bool? ignoreDisabledPrinters = true, bool exactMatch = false);
        Task<List<IADPrinter>> FindPrintersByStringAsync(string? searchTerm, bool? ignoreDisabledPrinters = true, bool exactMatch = false);
    }
}