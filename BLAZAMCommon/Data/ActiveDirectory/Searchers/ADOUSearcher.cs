using BLAZAM.Common.Data.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data.ActiveDirectory.Models;
using System.DirectoryServices;

namespace BLAZAM.Common.Data.ActiveDirectory.Searchers
{
    public class ADOUSearcher : ADSearcher, IADOUSearcher
    {

        protected ADSearch NewSearch { get { return new ADSearch() { ObjectTypeFilter = ActiveDirectoryObjectType.OU }; } }

        public async Task<IADOrganizationalUnit> GetApplicationRootOU()
        {
            IADOrganizationalUnit TopLevel = new ADOrganizationalUnit();

            await TopLevel.Parse(Directory.GetDirectoryEntry(), Directory);
            _ = TopLevel.Children;
            return TopLevel;
        }

        public ADOUSearcher(IActiveDirectory directory) : base(directory)
        {
        }

        public async Task<List<IADOrganizationalUnit>> FindOuByStringAsync(string searchTerm)
        {
            var search = NewSearch;
            search.GeneralSearchTerm = searchTerm;
            return await search.SearchAsync<ADOrganizationalUnit, IADOrganizationalUnit>();
            //  return await Task.Run(() =>
            // {
            //    return FindOuByString(searchTerm);
            //});
        }

        public List<IADOrganizationalUnit> FindOuByString(string searchTerm)
        {
            var search = NewSearch;
            search.GeneralSearchTerm = searchTerm;
            var temp = search.Search<ADOrganizationalUnit, IADOrganizationalUnit>();
            return temp;
            // string GroupSearchFieldsQuery = "(|(distinguishedName=" + searchTerm + ")(samaccountname=*" + searchTerm + "*)(displayName=*" + searchTerm + "*)(name=*" + searchTerm + "*))";
            // return new List<IADOrganizationalUnit>(ConvertTo<ADOrganizationalUnit>(SearchObjects(GroupSearchFieldsQuery, ActiveDirectoryObjectType.OU, 25)));
        }

        public IADOrganizationalUnit? FindOuByDN(string searchTerm)
        {

            return FindOuByString(searchTerm).OrderBy(x => x.DN).FirstOrDefault();
        }

        public List<IADOrganizationalUnit> FindSubOusByDN(string? searchBaseDN) => new List<IADOrganizationalUnit>(ConvertTo<ADOrganizationalUnit>(SearchObjects(searchBaseDN, "", ActiveDirectoryObjectType.OU, 1000, true, SearchScope.OneLevel)));

        public List<IADUser> FindSubUsersByDN(string searchBaseDN) => new List<IADUser>(ConvertTo<ADUser>(SearchObjects(searchBaseDN, "", ActiveDirectoryObjectType.User, 1000, true, SearchScope.OneLevel)));

        public List<IADComputer> FindSubComputerByDN(string searchBaseDN) => new List<IADComputer>(ConvertTo<ADComputer>(SearchObjects(searchBaseDN, "", ActiveDirectoryObjectType.Computer, 1000, true, SearchScope.OneLevel)));

        public List<IADGroup> FindSubGroupsByDN(string searchBaseDN) => new List<IADGroup>(ConvertTo<ADGroup>(SearchObjects(searchBaseDN, "", ActiveDirectoryObjectType.Group, 1000, true, SearchScope.OneLevel)));



        public async Task<List<IADOrganizationalUnit>> FindNewOUsAsync()
        {
            return await Task.Run(() =>
            {
                return FindNewOUs();
            });
        }

        public List<IADOrganizationalUnit> FindNewOUs()
        {

            var threeMonthsAgo = DateTime.Today - TimeSpan.FromDays(90);
            var results = new ADSearch()
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
