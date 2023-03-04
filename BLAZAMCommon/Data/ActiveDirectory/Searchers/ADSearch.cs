using BLAZAM.Common.Data.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data.ActiveDirectory.Models;
using BLAZAM.Common.Data.Database;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.IdentityModel.Tokens;
using Microsoft.TeamFoundation.Work.WebApi.Exceptions;
using Microsoft.VisualStudio.Services.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Common.Data.ActiveDirectory.Searchers
{
    public enum SearchState { Ready, Started, Collecting, Completed };

    public class ADSearch
    {
        private string? samAccountName;
        private string? lockoutTime;
        private string? sid;
        private string? created;
        private string? changed;
        private string? pwdLastSet;
        private string? cn;
        private string? dn;

        /// <summary>
        /// Indicates whether the resulting fields searched 
        /// should be an exact match of the terms provided
        /// </summary>
        public bool ExactMatch { get; set; }

        /// <summary>
        /// A string to find in the common name and username fields
        /// </summary>
        public string? GeneralSearchTerm { get; set; }

        public string? SamAccountName { get => samAccountName; set { samAccountName = value; GeneralSearchTerm = null; } }

        public string? LockoutTime { get => lockoutTime; set { lockoutTime = value; GeneralSearchTerm = null; } }

        public string? SID { get => sid; set { sid = value; GeneralSearchTerm = null; } }

        public string? DN { get => dn; set { dn = value; GeneralSearchTerm = null; } }

        public string? Created { get => created; set { created = value; GeneralSearchTerm = null; } }

        public string? Changed { get => changed; set { changed = value; GeneralSearchTerm = null; } }

        public string? PasswordLastSet { get => pwdLastSet; set { pwdLastSet = value; GeneralSearchTerm = null; } }

        public string? CN { get => cn; set { cn = value; GeneralSearchTerm = null; } }

        public string? MemberOf { get; set; }


        /// <summary>
        /// Allows for cancelling the search
        /// </summary>
        private CancellationTokenSource tokenSource { get; set; } = new CancellationTokenSource();

        /// <summary>
        /// The ldap query filter that filters by fields
        /// </summary>
        public string FilterQuery { get; set; }
        /// <summary>
        /// The search root and authenticated <see cref="DirectoryEntry"/>
        /// </summary>
        public DirectoryEntry SearchRoot { get; set; }
        /// <summary>
        /// Indicates whether the search is single level or recursive default is recursive
        /// </summary>
        public System.DirectoryServices.SearchScope SearchScope { get; set; } = System.DirectoryServices.SearchScope.Subtree;

        /// <summary>
        /// Indicates the current state of this search
        /// </summary>
        public SearchState SearchState { get; set; } = SearchState.Ready;

        /// <summary>
        /// The realtime results of this search. 
        /// <para>Check <see cref="SearchState"/>
        /// or listen to <see cref="OnSearchCompleted"/>
        /// to confirm search is completed and no more results are coming.</para>
        /// </summary>
        public AppEvent<IEnumerable<IDirectoryModel>> ResultsCollected { get; set; }
        /// <summary>
        /// Event fired when all results have been collected, or an error occurred
        /// </summary>
        public AppEvent OnSearchCompleted { get; set; }
        public AppEvent OnSearchStarted { get; set; }
        public ActiveDirectoryObjectType? ObjectTypeFilter { get; set; }
        public bool? EnabledOnly { get; set; }
        public int MaxResults { get; set; } = 5000;
        public List<SearchResult> SearchResults { get; set; } = new();
        public List<IDirectoryModel> Results { get; set; } = new();
        public string LdapQuery { get; private set; }
        public TimeSpan SearchTime { get; set; }
        public bool SearchDeleted { get; set; } = false;

        public async Task<List<I>> SearchAsync<T, I>() where T : I, IDirectoryModel, new()
        {
            return await Task.Run(() =>
            {
                return Search<T, I>();
            });
        }

        public List<IDirectoryModel> Search()
        {
            return Search<DirectoryModel, IDirectoryModel>();
        }

        public async Task<List<IDirectoryModel>> SearchAsync()
        {
            return await SearchAsync<DirectoryModel, IDirectoryModel>();
        }

        /// <summary>
        /// Executes a search in Active Directory using the configured properties of this object.
        /// </summary>
        /// <typeparam name="TObject">The object type to convert search results to</typeparam>
        /// <typeparam name="TInterface">The interface type to case converted search results to</typeparam>
        /// <returns>A list of search results converted and casted to supplied types</returns>
        public List<TInterface> Search<TObject, TInterface>() where TObject : TInterface, IDirectoryModel, new()
        {
            var startTime = DateTime.Now;
            SearchState = SearchState.Started;
            OnSearchStarted?.Invoke();
            tokenSource = new();
            Results.Clear();
            DirectorySearcher searcher;
            try
            {
                if (SearchRoot == null)
                    SearchRoot = ActiveDirectoryContext.Instance.GetDirectoryEntry(DatabaseCache.ActiveDirectorySettings?.ApplicationBaseDN);
                var pageOffset = 1;
                var pageSize = 40;
                searcher = new DirectorySearcher(SearchRoot)
                {
                    VirtualListView = new DirectoryVirtualListView(0, pageSize - 1, pageOffset)
                };

                searcher.Filter = "(&(|(objectClass=user)(objectClass=group)(objectCategory=computer)(objectClass=organizationalUnit)))";
                if (SearchDeleted)
                    searcher.Filter = searcher.Filter.Substring(0, searcher.Filter.Length - 1) + "(isDeleted=TRUE)" + ")";

                switch (ObjectTypeFilter)
                {
                    case ActiveDirectoryObjectType.All:
                    case null:
                        if (GeneralSearchTerm != null)
                            FilterQuery = "(|(samaccountname=*" + GeneralSearchTerm + "*)(cn=*" + GeneralSearchTerm + "*)(distinguishedName=" + GeneralSearchTerm + ")(givenname=*" + GeneralSearchTerm + "*)(sn=*" + GeneralSearchTerm + "*)(displayName=*" + GeneralSearchTerm + "*)(proxyAddresses=*" + GeneralSearchTerm + "*))";
                        break;

                    case ActiveDirectoryObjectType.Group:
                        searcher.Filter = "(&(objectCategory=group)(objectClass=group))";
                        if (GeneralSearchTerm != null)
                            FilterQuery = "(|(samaccountname=*" + GeneralSearchTerm + "*)(displayName=*" + GeneralSearchTerm + "*)(name=*" + GeneralSearchTerm + "*)(cn=*" + GeneralSearchTerm + "*))";
                     
                        break;
                    case ActiveDirectoryObjectType.User:
                        searcher.Filter = "(&(objectCategory=person)(objectClass=user))";
                        if (EnabledOnly == true)
                        {
                            searcher.Filter = "(&(objectCategory=person)(objectClass=user)(!userAccountControl:1.2.840.113556.1.4.803:=2))";
                        }
                        if (GeneralSearchTerm != null)
                            FilterQuery = "(|(samaccountname=*" + GeneralSearchTerm + "*)(givenname=*" + GeneralSearchTerm + "*)(sn=*" + GeneralSearchTerm + "*)(displayName=*" + GeneralSearchTerm + "*)(anr=*" + GeneralSearchTerm + "*)(proxyAddresses=*" + GeneralSearchTerm + "*))";
                      

                        break;
                    case ActiveDirectoryObjectType.Computer:
                        searcher.Filter = "(&(objectCategory=computer))";
                        if (EnabledOnly == true)
                        {
                            searcher.Filter = "(&(objectCategory=computer)(!userAccountControl:1.2.840.113556.1.4.803:=2))";
                        }
                        if (GeneralSearchTerm != null)
                            FilterQuery = "(|(samaccountname=*" + GeneralSearchTerm + "*)(anr=*" + GeneralSearchTerm + "*)(distinguishedName=*" + GeneralSearchTerm + "*))";
                  
                        break;
                    case ActiveDirectoryObjectType.OU:
                        searcher.VirtualListView = null;
                        searcher.Filter = "(&(objectCategory=organizationalUnit))";
                        if (GeneralSearchTerm != null)
                            FilterQuery = "(|(distinguishedName=" + GeneralSearchTerm + ")(displayName=*" + GeneralSearchTerm + "*)(name=*" + GeneralSearchTerm + "*)(cn=*" + GeneralSearchTerm + "*))";

                        break;
                }
                if (EnabledOnly == true)
                {
                    searcher.Filter = searcher.Filter.Substring(0, searcher.Filter.Length - 1) + "(!userAccountControl:1.2.840.113556.1.4.803:= 2))";
                }


                if (!FilterQuery.IsNullOrEmpty())
                    if (ExactMatch)
                        FilterQuery = FilterQuery.Replace("*", "");

                if (FilterQuery == null && GeneralSearchTerm == null)
                {
                    FilterQuery = "";
                    //FilterQuery = "(&(";

                    if (!CN.IsNullOrEmpty())
                        FilterQuery += $"(cn=*{CN}*)";
                    if (!Changed.IsNullOrEmpty())
                        FilterQuery += $"(whenChanged>=\"{Changed}\")";
                    if (!SamAccountName.IsNullOrEmpty())
                        FilterQuery += $"(samaccountname=*{SamAccountName}*)";
                    if (!LockoutTime.IsNullOrEmpty())
                        FilterQuery += $"(lockoutTime>={LockoutTime})";
                    if (!DN.IsNullOrEmpty())
                        FilterQuery += $"(distinguishedName={DN})";
                    if (!MemberOf.IsNullOrEmpty())
                        FilterQuery += $"(memberOf=*{DN})*";
                    if (!SID.IsNullOrEmpty())
                        FilterQuery += $"(objectSid={SID})";




                    //FilterQuery += ")";

                }
  
                if (!SearchDeleted)
                {
                    searcher.PropertiesToLoad.Add("samaccountname");
                    searcher.PropertiesToLoad.Add("distinguishedName");
                    searcher.PropertiesToLoad.Add("objectSID");
                    searcher.PropertiesToLoad.Add("objectclass");
                    searcher.PropertiesToLoad.Add("cn");
                    searcher.PropertiesToLoad.Add("name");
                }
                if (SearchDeleted)
                    searcher.Tombstone = true;
                //searcher.Asynchronous = true;
                searcher.SizeLimit = MaxResults;
                searcher.Filter = searcher.Filter.Substring(0, searcher.Filter.Length - 1) + FilterQuery + ")";
                LdapQuery = searcher.Filter;
                SearchTime = DateTime.Now - startTime;
                //searcher.PageSize = 50;
                searcher.Sort = new SortOption("cn", SortDirection.Ascending);


                bool moreResults = true;
                SearchState = SearchState.Collecting;
                SearchResultCollection lastResults = searcher.FindAll();
                SearchResult lastResult2s = searcher.FindOne();
                try
                {
                    var count = lastResults.Count;
                }
                catch
                {
                    searcher.VirtualListView = null;
                    lastResults = searcher.FindAll();
                }
                SearchTime = DateTime.Now - startTime;

                AddResults<TObject, TInterface>(lastResults);
                SearchTime = DateTime.Now - startTime;
                if (ObjectTypeFilter != ActiveDirectoryObjectType.OU)
                {
                    var approxTotal = searcher.VirtualListView.ApproximateTotal;
                    var progress = 0;
                    if (approxTotal > 0)
                        progress = SearchResults.Count / approxTotal;
                }
                if (lastResults.Count < pageSize)
                    moreResults = false;

                while (moreResults && !tokenSource.IsCancellationRequested)
                {
                    searcher.VirtualListView.Offset += pageSize;
                    lastResults = searcher.FindAll();
                    AddResults<TObject, TInterface>(lastResults);
                    if (lastResults.Count < pageSize)
                        moreResults = false;
                    SearchTime = DateTime.Now - startTime;

                }

                SearchState = SearchState.Completed;
                SearchTime = DateTime.Now - startTime;

                OnSearchCompleted?.Invoke();



                return Results.Cast<TInterface>().ToList();

                // return result;

            }
            catch (Exception ex)
            {
                Loggers.ActiveDirectryLogger.Error("Directory Entry failed to connect", ex);
            }

            SearchState = SearchState.Completed;
            SearchTime = DateTime.Now - startTime;

            OnSearchCompleted?.Invoke();

            return null;
            // Set the filter to look for a specific user


        }
        public void Cancel()
        {
            tokenSource.Cancel();
        }
        private void AddResults<T, I>(SearchResultCollection lastResults) where T : I, IDirectoryModel, new()
        {

            //var last = new List<I>((IEnumerable<I>)ConvertTo<T>(lastResults));

            // var last = ConvertTo<T, I>(lastResults).Cast<IDirectoryModel>();
            var last = Encapsulate(lastResults);
            Results.AddRange(last);

            ResultsCollected?.Invoke(last);

        }

        public List<I> ConvertTo<T, I>(ICollection r) where T : I, IDirectoryModel, new()
        {
            return new List<I>((IEnumerable<I>)ConvertTo<T>(r));
        }

        public List<T> ConvertTo<T>(ICollection r) where T : IDirectoryModel, new()
        {
            List<T> objects = new List<T>();


            if (r != null && r.Count > 0)
            {

                foreach (SearchResult sr in r)
                {

                    var o = new T();

                    o.Parse(sr, ActiveDirectoryContext.Instance);

                    objects.Add(o);
                }
            }
            return objects;
        }
        public List<IDirectoryModel> Encapsulate(SearchResultCollection r)
        {
            List<IDirectoryModel> objects = new List<IDirectoryModel>();


            if (r != null && r.Count > 0)
            {
                IDirectoryModel thisObject = null;
                foreach (SearchResult sr in r)
                {
                    if (sr.Properties["objectClass"].Contains("top"))
                    {
                        if (sr.Properties["objectClass"].Contains("computer"))
                        {
                            thisObject = new ADComputer();
                            thisObject.Parse(sr, ActiveDirectoryContext.Instance);
                        }
                        else if (sr.Properties["objectClass"].Contains("user"))
                        {
                            thisObject = new ADUser();
                            thisObject.Parse(sr, ActiveDirectoryContext.Instance);
                        }
                        else if (sr.Properties["objectClass"].Contains("organizationalUnit"))
                        {
                            thisObject = new ADOrganizationalUnit();
                            thisObject.Parse(sr, ActiveDirectoryContext.Instance);
                        }
                        else if (sr.Properties["objectClass"].Contains("group"))
                        {
                            thisObject = new ADGroup();
                            thisObject.Parse(sr, ActiveDirectoryContext.Instance);
                        }
                        if (thisObject != null)
                            objects.Add(thisObject);
                    }
                }
            }
            return objects;
        }
    }
}
