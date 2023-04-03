using BLAZAM.Common.Data.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data.Database;
using BLAZAM.Common.Data.Services;
using System.DirectoryServices;

namespace BLAZAM.Common.Data.ActiveDirectory.Searchers
{
    public class ADSearcher
    {

        protected IActiveDirectory Directory;

        public ADSearcher(IActiveDirectory directory)
        {
            Directory = directory;
        }
        protected virtual SearchResultCollection SearchObjects(
            string fieldQuery,
            ActiveDirectoryObjectType searchType,
            int returnCount = 5,
            bool? enabledOnly = true
            ) => SearchObjects(null, fieldQuery, searchType, returnCount, enabledOnly);




       


        protected virtual SearchResultCollection SearchObjects(
            string? searchBaseDN,
            string fieldQuery,
            ActiveDirectoryObjectType? searchType,

            int returnCount = 5,

            bool? enabledOnly = true,
            SearchScope searchScope = SearchScope.Subtree
            )
        {



            //if (enabledOnly == false) Don't do this to allow disabled only searches
            //if trying to find disabled users, set to null to include both enabled and disabled
            //enabledOnly = null;
            DirectorySearcher searcher;
            try
            {


              /*

                ADSearch search = new ADSearch();

                search.ObjectTypeFilter = searchType;
                search.SearchRoot = Directory.GetDirectoryEntry(searchBaseDN);
                search.FilterQuery = fieldQuery;
                search.MaxResults = returnCount;
                search.SearchScope = searchScope;
                search.EnabledOnly = enabledOnly;
                return search.Search();



                */


                searcher = new DirectorySearcher(Directory.GetDirectoryEntry(searchBaseDN));
                searcher.Filter = "(&(objectClass=*))";
                switch (searchType)
                {
                    case ActiveDirectoryObjectType.Group:
                        searcher.Filter = "(&(objectCategory=group)(objectClass=group))";

                        break;
                    case ActiveDirectoryObjectType.User:
                        searcher.Filter = "(&(objectCategory=person)(objectClass=user))";
                        if (enabledOnly == true)
                        {
                            searcher.Filter = "(&(objectCategory=person)(objectClass=user)(!userAccountControl:1.2.840.113556.1.4.803:=2))";
                        }
                        break;
                    case ActiveDirectoryObjectType.Computer:
                        searcher.Filter = "(&(objectCategory=computer))";
                        if (enabledOnly == true)
                        {
                            searcher.Filter = "(&(objectCategory=computer)(!userAccountControl:1.2.840.113556.1.4.803:=2))";
                        }
                        break;
                    case ActiveDirectoryObjectType.OU:
                        searcher.Filter = "(&(objectCategory=organizationalUnit))";

                        break;


                }







                searcher.SearchScope = searchScope;
                searcher.PropertiesToLoad.Add("samaccountname");
                searcher.PropertiesToLoad.Add("distinguishedName");
                searcher.PropertiesToLoad.Add("objectSID");
                searcher.PropertiesToLoad.Add("objectclass");
                searcher.PropertiesToLoad.Add("cn");
                searcher.PropertiesToLoad.Add("name");
                searcher.SizeLimit = returnCount;
                //searcher.Asynchronous = true;
                //se.SizeLimit = returnCount;
                searcher.Filter = searcher.Filter.Substring(0, searcher.Filter.Length - 1) + fieldQuery + ")";
                var result = searcher.FindAll();

                return result;
            }
            catch (Exception ex)
            {
                Loggers.ActiveDirectryLogger.Error("Directory Entry failed to connect", ex);
            }
            return null;
            // Set the filter to look for a specific user



        }







        protected SearchResultCollection SearchObjectBySID(string sid) => SearchObjects(null, "(objectSid=" + sid + ")", null, 1, false);

        protected List<T> ConvertTo<T>(SearchResultCollection r) where T : IDirectoryEntryAdapter, new()
        {
            List<T> objects = new List<T>();


            if (r != null && r.Count > 0)
            {

                foreach (SearchResult sr in r)
                {
                    var o = new T();
                    o.Parse(sr, Directory);

                    objects.Add(o);
                }
            }
            return objects;
        }



    }
}
