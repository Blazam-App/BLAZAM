using BLAZAM.ActiveDirectory.Adapters;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using System.DirectoryServices;

namespace BLAZAM.ActiveDirectory.Searchers
{
    public class ADOUSearcher : ADSearcher, IADOUSearcher
    {

        protected ADSearch NewSearch { get { return new ADSearch(Directory) { ObjectTypeFilter = ActiveDirectoryObjectType.OU }; } }

        public IADOrganizationalUnit GetApplicationRootOU()
        {
            IADOrganizationalUnit TopLevel = new ADOrganizationalUnit();

            TopLevel.Parse(directory: Directory, directoryEntry: Directory.GetDirectoryEntry());
            _ = TopLevel.SubOUs;
            return TopLevel;
        }

        public ADOUSearcher(IActiveDirectoryContext directory) : base(directory)
        {
        }

        public async Task<List<IADOrganizationalUnit>> FindOuByStringAsync(string searchTerm)
        {
            var search = NewSearch;
            search.GeneralSearchTerm = searchTerm;
            return await search.SearchAsync<ADOrganizationalUnit, IADOrganizationalUnit>();

        }

        public List<IADOrganizationalUnit> FindOuByString(string searchTerm)
        {
            var search = NewSearch;
            search.GeneralSearchTerm = searchTerm;
            var temp = search.Search<ADOrganizationalUnit, IADOrganizationalUnit>();
            return temp;
        }

        public IADOrganizationalUnit? FindOuByDN(string searchTerm)
        {

            return FindOuByString(searchTerm).OrderBy(x => x.DN).FirstOrDefault();
        }

        public List<IADOrganizationalUnit> FindSubOusByDN(string? searchTerm) => SearchObjects(searchTerm, "", ActiveDirectoryObjectType.OU, 1000, true, SearchScope.OneLevel).Cast<IADOrganizationalUnit>().ToList();

        public List<IADUser> FindSubUsersByDN(string searchTerm) => SearchObjects(searchTerm, "", ActiveDirectoryObjectType.User, 1000, true, SearchScope.OneLevel).Cast<IADUser>().ToList();

        public List<IADComputer> FindSubComputerByDN(string searchTerm)
        {
            var search = NewSearch;
            search.GeneralSearchTerm = searchTerm;
            var temp = search.Search<ADComputer, IADComputer>();
            return temp;
        }

        public List<IADGroup> FindSubGroupsByDN(string searchTerm) => SearchObjects(searchTerm, "", ActiveDirectoryObjectType.Group, 1000, true, SearchScope.OneLevel).Cast<IADGroup>().ToList();



        public async Task<List<IADOrganizationalUnit>> FindNewOUsAsync(int maxAgeInDays = 14)
        {
            return await Task.Run(() =>
            {
                return FindNewOUs(maxAgeInDays);
            });
        }

        public List<IADOrganizationalUnit> FindNewOUs(int maxAgeInDays = 14)
        {

            var threeMonthsAgo = DateTime.Today - TimeSpan.FromDays(maxAgeInDays);
            var results = new ADSearch(Directory)
            {
                ObjectTypeFilter = ActiveDirectoryObjectType.OU,
                Fields = new()
                {
                    Created = threeMonthsAgo
                }

            }.Search<ADOrganizationalUnit, IADOrganizationalUnit>();
            return results.OrderByDescending(u => u.Created).ToList();

        }


    }
}
