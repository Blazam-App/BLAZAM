using BLAZAM.ActiveDirectory;
using BLAZAM.ActiveDirectory.Adapters;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using BLAZAM.Common.Data.Services;

namespace BLAZAM.ActiveDirectory.Searchers
{
    public class ADComputerSearcher : ADSearcher, IADComputerSearcher
    {
        public WmiFactory WmiFactory { get; set; }

        public ADComputerSearcher(IActiveDirectoryContext directory, WmiFactory wmiFactory) : base(directory)
        {
            WmiFactory = wmiFactory;
        }
        public async Task<List<IADComputer>> FindByStringAsync(string searchTerm, bool ignoreDisabled = true)
        {
            return await Task.Run(() =>
            {
                return FindByString(searchTerm, ignoreDisabled);
            });
        }
        public List<IADComputer> FindByString(string searchTerm, bool ignoreDisabled = true)
        {
            return new ADSearch()
            {
                ObjectTypeFilter = ActiveDirectoryObjectType.Computer,
                EnabledOnly = ignoreDisabled,
                GeneralSearchTerm = searchTerm

            }.Search<ADComputer, IADComputer>();

        }

        public async Task<List<IADComputer>> FindNewComputersAsync(int maxAgeInDays = 14, bool ignoreDisabledComputers = false)
        {
            return await Task.Run(() =>
            {
                return FindNewComputers(maxAgeInDays, ignoreDisabledComputers);
            });
        }

        public List<IADComputer> FindNewComputers(int maxAgeInDays = 14, bool ignoreDisabledComputers = false)
        {

            var threeMonthsAgo = DateTime.Today - TimeSpan.FromDays(maxAgeInDays);
            var results = new ADSearch()
            {
                ObjectTypeFilter = ActiveDirectoryObjectType.Computer,
                EnabledOnly = ignoreDisabledComputers,
                Fields = new()
                {
                    Created = threeMonthsAgo
                }

            }.Search<ADComputer, IADComputer>();
            return results.OrderByDescending(u => u.Created).ToList();


        }
    }
}
