using BLAZAM.ActiveDirectory;
using BLAZAM.ActiveDirectory.Adapters;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using BLAZAM.Common.Data.Database;
using BLAZAM.Database.Context;
using BLAZAM.Helpers;
using BLAZAM.Logger;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.ActiveDirectory.Searchers
{
    /// <summary>
    /// Represents the state of the search process.
    /// </summary>
    public enum SearchState { Ready, Started, Collecting, Completed };

    /// <summary>
    /// The ADSearch class provides a powerful and flexible mechanism for performing searches within an Active Directory environment.
    /// By configuring various search parameters, users can query the directory for specific types of objects, such as users, groups, 
    /// computers, organizational units, and more. This class leverages LDAP queries to retrieve and filter results efficiently.
    /// </summary>
    public class ADSearch : SearchBase
    {

        public ADSearchFields Fields { get; set; } = new();



        /// <summary>
        /// Indicates whether the resulting fields searched 
        /// should be an exact match of the terms provided
        /// </summary>
        public bool ExactMatch { get; set; }

        /// <summary>
        /// A string to find in the common name and username fields
        /// </summary>
        public string? GeneralSearchTerm { get; set; }

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
        /// The realtime results of this search. 
        /// <para>Check <see cref="SearchState"/>
        /// or listen to <see cref="OnSearchCompleted"/>
        /// to confirm search is completed and no more results are coming.</para>
        /// </summary>
        public AppEvent<IEnumerable<IDirectoryEntryAdapter>> ResultsCollected { get; set; }
        
        int PageSize = 40;
        
        public ActiveDirectoryObjectType? ObjectTypeFilter { get; set; }
        public bool? EnabledOnly { get; set; }
        public int MaxResults { get; set; } = 50;
        private List<SearchResult> _searchResults = new();

        public List<IDirectoryEntryAdapter> Results { get; set; } = new();
        public string LdapQuery { get; private set; }
        public bool SearchDeleted { get; set; } = false;

        public async Task<List<I>> SearchAsync<T, I>(CancellationToken? token = null) where T : I, IDirectoryEntryAdapter, new()
        {
            return await Task.Run(() =>
            {
                return Search<T, I>(token);
            });
        }
        /// <summary>
        /// Searches ambiguously for all object types
        /// </summary>
        /// <returns></returns>
        public List<IDirectoryEntryAdapter> Search()
        {
            return Search<DirectoryEntryAdapter, IDirectoryEntryAdapter>();
        }

        public async Task<List<IDirectoryEntryAdapter>> SearchAsync()
        {
            return await SearchAsync<DirectoryEntryAdapter, IDirectoryEntryAdapter>();
        }

        /// <summary>
        /// Executes a search in Active Directory using the configured properties of this object.
        /// </summary>
        /// <typeparam name="TObject">The object type to convert search results to</typeparam>
        /// <typeparam name="TInterface">The interface type to case converted search results to</typeparam>
        /// <returns>A list of search results converted and casted to supplied types</returns>
        public List<TInterface> Search<TObject, TInterface>(CancellationToken? token = null) where TObject : TInterface, IDirectoryEntryAdapter, new()
        {
            if (token != null) cancellationToken = token;
            else cancellationToken = new CancellationToken();
            if (cancellationToken?.IsCancellationRequested == true) 
                return new();
            DateTime startTime = InitializeSearch();
            DirectorySearcher searcher;
            try
            {
                SearchRoot ??= ActiveDirectoryContext.Instance.GetDirectoryEntry(DatabaseCache.ActiveDirectorySettings?.ApplicationBaseDN);
                var pageOffset = 1;
                
                searcher = new DirectorySearcher(SearchRoot)
                {
                    //TODO Ensure bbroken
                    //Make sure this is not  usable
                    //Seems to never pull ou's
                    //VirtualListView = new DirectoryVirtualListView(0, pageSize - 1, pageOffset),
                    SearchScope=SearchScope,
                    SizeLimit= MaxResults,
                    Filter = "(&(|(&(objectClass=user)(!userAccountControl:1.2.840.113556.1.4.803:=2))(objectClass=group)(&(objectCategory=computer)(!userAccountControl:1.2.840.113556.1.4.803:=2))(objectClass=organizationalUnit)(objectClass=printQueue)))"
                };
                if (EnabledOnly != true)
                {
                    searcher.Filter = searcher.Filter.Replace("(!userAccountControl:1.2.840.113556.1.4.803:=2)", "");
                }
                if (SearchDeleted)
                    searcher.Filter = searcher.Filter.Substring(0, searcher.Filter.Length - 1) + "(isDeleted=TRUE)" + ")";

                switch (ObjectTypeFilter)
                {
                    case ActiveDirectoryObjectType.All:
                    case null:
                        if (GeneralSearchTerm != null)
                            FilterQuery = "(|(samaccountname=*" + GeneralSearchTerm + "*)(cn=*" + GeneralSearchTerm + "*)(distinguishedName=" + GeneralSearchTerm + ")(givenname=*" + GeneralSearchTerm + "*)(sn=*" + GeneralSearchTerm + "*)(displayName=*" + GeneralSearchTerm + "*)(proxyAddresses=*" + GeneralSearchTerm + "*)(ou=*" + GeneralSearchTerm + "*)(name=*" + GeneralSearchTerm + "*))";
                        break;
                    case ActiveDirectoryObjectType.Printer:
                        searcher.Filter = "(&(objectClass=printQueue))";
                        if (GeneralSearchTerm != null)
                            FilterQuery = "(|(samaccountname=*" + GeneralSearchTerm + "*)(displayName=*" + GeneralSearchTerm + "*)(name=*" + GeneralSearchTerm + "*)(cn=*" + GeneralSearchTerm + "*))";

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
                            FilterQuery = "(|(distinguishedName=" + GeneralSearchTerm + ")(ou=*" + GeneralSearchTerm + "*)(name=*" + GeneralSearchTerm + "*)(displayName=*" + GeneralSearchTerm + "*)(cn=*" + GeneralSearchTerm + "*))";

                        break;
                }
              

                if (!FilterQuery.IsNullOrEmpty() && ExactMatch)
                    FilterQuery = FilterQuery.Replace("*", "");

                if (GeneralSearchTerm == null)
                {
                    FilterQuery = "";

                    if (!Fields.CN.IsNullOrEmpty())
                        FilterQuery += $"(cn=*{Fields.CN}*)";
                    if (Fields.Changed != null)
                        FilterQuery += $"(whenChanged>={Fields.Changed.Value.ToString("yyyyMMddHHmmss.fZ")})";
                    if (Fields.Created != null)
                        FilterQuery += $"(whenCreated>={Fields.Created.Value.ToString("yyyyMMddHHmmss.fZ")})";
                    if (!Fields.SamAccountName.IsNullOrEmpty())
                        FilterQuery += $"(samaccountname=*{Fields.SamAccountName}*)";
                    if (Fields.LockoutTime != null)
                        FilterQuery += $"(lockoutTime>={Fields.LockoutTime})";
                    if (!Fields.DN.IsNullOrEmpty())
                        FilterQuery += $"(distinguishedName={Fields.DN})";
                    if (!Fields.MemberOf.IsNullOrEmpty())
                        FilterQuery += $"(memberOf=*{Fields.DN})*";
                    if (!Fields.SID.IsNullOrEmpty())
                        FilterQuery += $"(objectSid={Fields.SID})";
                    if (Fields.NestedMemberOf != null)
                        FilterQuery += $"(memberOf:1.2.840.113556.1.4.1941:={Fields.NestedMemberOf.DN})";




                }
                if (cancellationToken?.IsCancellationRequested == true) 
                    return new();

                PrepareSearcher(searcher);
                if (cancellationToken?.IsCancellationRequested == true) 
                    return new();

                SearchTime = DateTime.Now - startTime;

                PerformSearch<TObject, TInterface>(startTime, searcher, PageSize);

                if (cancellationToken?.IsCancellationRequested == true) 
                    return new();

                SearchState = SearchState.Completed;
                SearchTime = DateTime.Now - startTime;



                if (cancellationToken?.IsCancellationRequested == true) 
                    return new();

                OnSearchCompleted?.Invoke();


                return Results.Cast<TInterface>().ToList();


            }
            catch (Exception ex)
            {
                Loggers.ActiveDirectryLogger.Error("Directory Entry failed to connect {@Error}", ex);
            }

            SearchState = SearchState.Completed;
            SearchTime = DateTime.Now - startTime;

            OnSearchCompleted?.Invoke();

            return new List<TInterface>();


        }

        private DateTime InitializeSearch()
        {
            var startTime = DateTime.Now;
            SearchState = SearchState.Started;
            OnSearchStarted?.Invoke();
            cancellationToken = new();
            Results.Clear();
            return startTime;
        }

        private void PerformSearch<TObject, TInterface>(DateTime startTime, DirectorySearcher searcher, int pageSize) where TObject : IDirectoryEntryAdapter, TInterface, new()
        {

            bool moreResults = true;
            SearchState = SearchState.Collecting;
            SearchResultCollection lastResults;
            try
            {
                if (cancellationToken?.IsCancellationRequested == true) return;

                lastResults = searcher.FindAll();
                if (cancellationToken?.IsCancellationRequested == true) return;

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
                var approxTotal = searcher.VirtualListView?.ApproximateTotal;
                var progress = 0;
                if (approxTotal != null && approxTotal > 0)
                    progress = _searchResults.Count / approxTotal.Value;
            }
            if (lastResults.Count < pageSize)
                moreResults = false;

            while (moreResults && cancellationToken?.IsCancellationRequested != true && searcher.VirtualListView!=null)
            {
                if (searcher.VirtualListView != null)
                    searcher.VirtualListView.Offset += pageSize;
                //else
                //    throw new ApplicationException("The searcher lost it's VirtualListView in the middle of searching!");
                lastResults = searcher.FindAll();
                AddResults<TObject, TInterface>(lastResults);
                if (searcher.VirtualListView==null || lastResults.Count < pageSize)
                    moreResults = false;

            }
            SearchTime = DateTime.Now - startTime;

            if (cancellationToken?.IsCancellationRequested == true) return;

        }

        private void PrepareSearcher(DirectorySearcher searcher)
        {
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
            {
                searcher.Tombstone = true;
                searcher.VirtualListView = new DirectoryVirtualListView(0, PageSize - 1, 1);

            }


            //searcher.Asynchronous = true;
            searcher.SizeLimit = MaxResults;
            searcher.Filter = searcher.Filter?.Substring(0, searcher.Filter.Length - 1) + FilterQuery + ")";
            LdapQuery = searcher.Filter;
            searcher.Sort = new SortOption("cn", SortDirection.Ascending);
        }
        /// <summary>
        /// Cancels the current search if still running
        /// </summary>
        public void Cancel()
        {
            Results.Clear();

            cancellationToken = new CancellationToken(true);

        }
        private void AddResults<T, I>(SearchResultCollection lastResults) where T : I, IDirectoryEntryAdapter, new()
        {


            var last = lastResults.Encapsulate();
            Results.AddRange(last);

            ResultsCollected?.Invoke(last);

        }

       
    }
}
