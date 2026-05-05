using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using BLAZAM.Logger;
using System.DirectoryServices;

namespace BLAZAM.ActiveDirectory.Searchers
{
    public class ADSearcher
    {

        protected IActiveDirectoryContext Directory;

        public ADSearcher(IActiveDirectoryContext directory)
        {
            Directory = directory;
        }
        protected virtual List<IDirectoryEntryAdapter>? SearchObjects(
            string fieldQuery,
            ActiveDirectoryObjectType searchType,
            int returnCount = 5,
            bool? enabledOnly = true
            ) => SearchObjects(null, fieldQuery, searchType, returnCount, enabledOnly);







        protected virtual List<IDirectoryEntryAdapter>? SearchObjects(
            string? searchBaseDN,
            string fieldQuery,
            ActiveDirectoryObjectType? searchType,
            int returnCount = 5,
            bool? enabledOnly = true,
            System.DirectoryServices.Protocols.SearchScope searchScope = System.DirectoryServices.Protocols.SearchScope.Subtree
            )
        {
            try
            {
                ADSearch search = new(Directory)
                {
                    ObjectTypeFilter = searchType,
                    SearchRoot = Directory.GetDirectoryEntry(searchBaseDN),
                    FilterQuery = fieldQuery,
                    MaxResults = returnCount,
                    SearchScope = searchScope,
                    EnabledOnly = enabledOnly
                };
                var results = search.Search();
                return results;
            }
            catch (Exception ex)
            {
                Loggers.ActiveDirectoryLogger.Error(ex, "Search failed");
            }
            return null;
        }







        protected List<IDirectoryEntryAdapter>? SearchObjectBySID(string sid) => SearchObjects(null, "(objectSid=" + sid + ")", null, 1, false);

        protected List<T> ConvertTo<T>(SearchResultCollection r) where T : IDirectoryEntryAdapter, new()
        {
            List<T> objects = [];


            if (r != null && r.Count > 0)
            {

                foreach (SearchResult sr in r)
                {
                    var o = new T();
                    o.Parse(directory: Directory, searchResult: sr);

                    objects.Add(o);
                }
            }
            return objects;
        }



    }
}
