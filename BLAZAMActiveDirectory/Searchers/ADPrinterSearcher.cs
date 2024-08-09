using BLAZAM.ActiveDirectory;
using BLAZAM.ActiveDirectory.Adapters;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;

namespace BLAZAM.ActiveDirectory.Searchers
{
    public class ADPrinterSearcher : ADSearcher, IADPrinterSearcher
    {

        public ADPrinterSearcher(IActiveDirectoryContext directory) : base(directory)
        {
        }

        public async Task<List<IADPrinter>> FindPrintersByStringAsync(string? searchTerm, bool? ignoreDisabledPrinters = true, bool exactMatch = false)
        {
            return await Task.Run(() =>
            {
                return FindPrintersByString(searchTerm, ignoreDisabledPrinters, exactMatch);
            });
        }

        public List<IADPrinter> FindPrintersByString(string? searchTerm, bool? ignoreDisabledPrinters = true, bool exactMatch = false)
        {
            return new ADSearch(Directory)
            {
                ObjectTypeFilter = ActiveDirectoryObjectType.Printer,
                EnabledOnly = ignoreDisabledPrinters,
                GeneralSearchTerm = searchTerm,
                ExactMatch = exactMatch

            }.Search<ADPrinter, IADPrinter>();
        }
        public IADPrinter? FindPrinterByName(string? searchTerm, bool? ignoreDisabledPrinters = true)
        {
            return new ADSearch(Directory)
            {
                ObjectTypeFilter = ActiveDirectoryObjectType.Printer,
                EnabledOnly = ignoreDisabledPrinters,
                Fields = new()
                {
                    SamAccountName = searchTerm
                },
                ExactMatch = true

            }.Search<ADPrinter, IADPrinter>().FirstOrDefault();

        }




        public async Task<List<IADPrinter>> FindNewPrintersAsync(int maxAgeInDays = 14, bool? ignoreDisabledPrinters = true)
        {
            return await Task.Run(() =>
            {
                return FindNewPrinters(maxAgeInDays, ignoreDisabledPrinters);
            });
        }

        public List<IADPrinter> FindNewPrinters(int maxAgeInDays = 14, bool? ignoreDisabledPrinters = true)
        {

            var threeMonthsAgo = DateTime.Today - TimeSpan.FromDays(maxAgeInDays);
            var results = new ADSearch(Directory)
            {
                ObjectTypeFilter = ActiveDirectoryObjectType.Printer,
                EnabledOnly = ignoreDisabledPrinters,
                Fields = new()
                {
                    Created = threeMonthsAgo
                }

            }.Search<ADPrinter, IADPrinter>();
            return results.OrderByDescending(u => u.Created).ToList();

        }

        public async Task<List<IADPrinter>> FindChangedPrintersAsync(bool? ignoreDisabledPrinters = true)
        {
            return await Task.Run(() =>
            {
                return FindChangedPrinters(ignoreDisabledPrinters);
            });
        }

        public List<IADPrinter> FindChangedPrinters(bool? ignoreDisabledPrinters = true, int daysBackToSearch = 90)
        {
            var threeMonthsAgo = DateTime.Today - TimeSpan.FromDays(daysBackToSearch);

            var tstamp = threeMonthsAgo.ToString("yyyyMMddHHmmss.fZ");
            string PrintersearchFieldsQuery = "(whenChanged>=" + tstamp + ")";

            return SearchObjects(PrintersearchFieldsQuery, ActiveDirectoryObjectType.User, 1000, ignoreDisabledPrinters).Cast<IADPrinter>().OrderByDescending(u => u.LastChanged).ToList();

        }


        public IADPrinter? FindPrintersByContainerName(string? searchTerm, bool? ignoreDisabledPrinters = true, bool exactMatch = false)
        {

            return new ADSearch(Directory)
            {
                ObjectTypeFilter = ActiveDirectoryObjectType.Printer,
                EnabledOnly = ignoreDisabledPrinters,
                Fields = new() { CN = searchTerm },
                ExactMatch = exactMatch

            }.Search<ADPrinter, IADPrinter>().FirstOrDefault();

        }
    }
}
