using BLAZAM.ActiveDirectory;
using BLAZAM.ActiveDirectory.Adapters;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using BLAZAM.Common.Data.Services;

namespace BLAZAM.ActiveDirectory.Searchers
{
    public class ADBitLockerSearcher : ADSearcher, IADBitLockerSearcher
    {

        public ADBitLockerSearcher(IActiveDirectoryContext directory) : base(directory)
        {
        }


        public List<IADBitLockerRecovery> FindByRecoveryId(string searchTerm)
        {
            var searchFields  = new ADSearchFields();
            searchFields.BitLockerRecoveryId = searchTerm;
            return new ADSearch(Directory)
            {
                ObjectTypeFilter = ActiveDirectoryObjectType.BitLocker,
                EnabledOnly = false,
                Fields = searchFields

            }.Search<ADBitLockerRecovery, IADBitLockerRecovery>();
        }

        public async Task<List<IADBitLockerRecovery>> FindByRecoveryIdAsync(string searchTerm)
        {
            return await Task.Run(() =>
            {
                return FindByRecoveryId(searchTerm);
            });
        }

        public List<IADBitLockerRecovery> FindByComputer(IADComputer computer)
        {
            var children = computer.Children;
            return children.Where(c => c is IADBitLockerRecovery).Cast<IADBitLockerRecovery>().ToList();
           
        }

        public async Task<List<IADBitLockerRecovery>> FindByComputerAsync(IADComputer computer)
        {
            return await Task.Run(() =>
            {
                return FindByComputer(computer);
            });
        }
    }
}
