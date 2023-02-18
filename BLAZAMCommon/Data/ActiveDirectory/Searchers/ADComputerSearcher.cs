using BLAZAM.Common.Data.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data.ActiveDirectory.Models;
using BLAZAM.Common.Data.Services;

namespace BLAZAM.Common.Data.ActiveDirectory.Searchers
{
    public class ADComputerSearcher : ADSearcher, IADComputerSearcher
    {
        public WmiFactoryService WmiFactory {get;set;}

        public ADComputerSearcher(IActiveDirectory directory,WmiFactoryService wmiFactory) : base(directory)
        {
            WmiFactory = wmiFactory;
        }
        public async Task<List<IADComputer>> FindByStringAsync(string searchTerm,bool ignoreDisabled=true)
        {
            return await Task.Run(() =>
            {
                return FindByString(searchTerm,ignoreDisabled);
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


    }
}
