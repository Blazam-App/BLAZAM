using BLAZAM.ActiveDirectory.Adapters;
using BLAZAM.ActiveDirectory.Data;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using BLAZAM.Database.Context;
using BLAZAM.Database.Models;
using BLAZAM.Database.Models.Rules;
using BLAZAM.Helpers;
using BLAZAM.Logger;
using Microsoft.IdentityModel.Tokens;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

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
        public List<ADFieldValue> FieldValues { get; set; } = new();


        /// <summary>
        /// Indicates whether the resulting fields searched 
        /// should be an exact match of the terms provided
        /// </summary>
        public bool ExactMatch { get; set; }
        private string? _generalSearchTerm;
        /// <summary>
        /// A string to find in the common name and username fields
        /// </summary>
        public string? GeneralSearchTerm
        {
            get => _generalSearchTerm;
            set => _generalSearchTerm = value.EscapeLdapSearchFilter();
        }

        /// <summary>
        /// The ldap query filter that filters by fields
        /// </summary>
        public string FilterQuery { get; set; }
        /// <summary>
        /// The search root and authenticated <see cref="DirectoryEntry"/>
        /// </summary>
        public IDirectoryEntry SearchRoot { get; set; }
        /// <summary>
        /// Indicates whether the search is single level or recursive default is recursive
        /// </summary>
        public System.DirectoryServices.Protocols.SearchScope SearchScope { get; set; } = System.DirectoryServices.Protocols.SearchScope.Subtree;

        /// <summary>
        /// The realtime results of this search. 
        /// <para>Check <see cref="SearchState"/>
        /// or listen to <see cref="OnSearchCompleted"/>
        /// to confirm search is completed and no more results are coming.</para>
        /// </summary>
        public AppDelegate<IEnumerable<IDirectoryEntryAdapter>> ResultsCollected { get; set; }

        private int PageSize = 100;

        public ActiveDirectoryObjectType? ObjectTypeFilter { get; set; }
        public bool? EnabledOnly { get; set; }
        public int MaxResults { get; set; } = 500;
        private List<SearchResult> _searchResults = new();

        public List<IDirectoryEntryAdapter> Results { get; set; } = new();
        public string LdapFilter { get; private set; }
        public bool SearchDeleted { get; set; } = false;
        public bool DisabledOnly { get; set; }

        private IActiveDirectoryContext? _currentUserActiveDirectoryContext;

        public ADSearch(IActiveDirectoryContext? currentUserActiveDirectoryContext)
        {
            _currentUserActiveDirectoryContext = currentUserActiveDirectoryContext;
        }

        public async Task<List<I>> SearchAsync<T, I>(CancellationToken? token = null) where T : I, IDirectoryEntryAdapter, new()
        {
            return await Task.Run(() =>
            {
                return Search<T, I>(token);
            });
        }

        public async Task<List<IDirectoryEntryAdapter>> SearchAsync()
        {
            return await SearchAsync<DirectoryEntryAdapter, IDirectoryEntryAdapter>();
        }

        /// <summary>
        /// Searches ambiguously for all object types
        /// </summary>
        /// <returns></returns>
        public List<IDirectoryEntryAdapter> Search()
        {
            return Search<DirectoryEntryAdapter, IDirectoryEntryAdapter>();
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
            InitializeSearch();
            DirectorySearcher searcher;
            try
            {
                SearchRoot ??= ActiveDirectoryContext.SystemInstance.GetDirectoryEntry(DatabaseCache.ActiveDirectorySettings?.ApplicationBaseDN);

                LdapFilter = "(&(|(objectClass=user)(objectClass=group)(objectClass=contact)(objectCategory=computer)(objectClass=organizationalUnit)(objectClass=printQueue)))";



                var pageOffset = 1;

                //searcher = new DirectorySearcher((SearchRoot as LdapDirectoryEntry)?.UnderlyingEntry)
                //{
                //    VirtualListView = new DirectoryVirtualListView(0, PageSize - 1, pageOffset),
                //    PageSize = PageSize,
                //    Sort = new SortOption(ActiveDirectoryFields.CanonicalName.FieldName, SortDirection.Ascending),
                //    SearchScope = SearchScope,
                //    SizeLimit = MaxResults,
                //    Filter = "(&(|(&(objectClass=user)(!userAccountControl:1.2.840.113556.1.4.803:=2))(objectClass=group)(objectClass=contact)(&(objectCategory=computer)(!userAccountControl:1.2.840.113556.1.4.803:=2))(objectClass=organizationalUnit)(objectClass=printQueue)))"
                //};
                if (EnabledOnly == false)
                {
                    //LdapFilter = LdapFilter.Replace("(!userAccountControl:1.2.840.113556.1.4.803:=2)", "");
                }
                else if (DisabledOnly == true)
                {
                    //LdapFilter = LdapFilter.Replace("(!userAccountControl:1.2.840.113556.1.4.803:=2)", "(userAccountControl:1.2.840.113556.1.4.803:=2)");

                }
                if (SearchDeleted)
                    LdapFilter = LdapFilter.Substring(0, LdapFilter.Length - 1) + "(isDeleted=TRUE)" + ")";

                switch (ObjectTypeFilter)
                {
                    case ActiveDirectoryObjectType.All:
                    case null:
                        if (GeneralSearchTerm != null)
                            FilterQuery = "(|(samaccountname=*" + GeneralSearchTerm + "*)(cn=*" + GeneralSearchTerm + "*)(distinguishedName=" + GeneralSearchTerm + ")(givenname=*" + GeneralSearchTerm + "*)(sn=*" + GeneralSearchTerm + "*)(displayName=*" + GeneralSearchTerm + "*)(name=*" + GeneralSearchTerm + "*)(mail=*" + GeneralSearchTerm + "*@*)(anr=*" + GeneralSearchTerm + "*))";
                        break;
                    case ActiveDirectoryObjectType.Printer:
                        LdapFilter = "(&(objectClass=printQueue))";
                        if (GeneralSearchTerm != null)
                            FilterQuery = "(|(samaccountname=*" + GeneralSearchTerm + "*)(displayName=*" + GeneralSearchTerm + "*)(name=*" + GeneralSearchTerm + "*)(cn=*" + GeneralSearchTerm + "*)(anr=*" + GeneralSearchTerm + "*))";

                        break;
                    case ActiveDirectoryObjectType.Group:
                        LdapFilter = "(&(objectCategory=group)(objectClass=group))";
                        if (GeneralSearchTerm != null)
                            FilterQuery = "(|(samaccountname=*" + GeneralSearchTerm + "*)(displayName=*" + GeneralSearchTerm + "*)(name=*" + GeneralSearchTerm + "*)(cn=*" + GeneralSearchTerm + "*)(mail=" + GeneralSearchTerm + "*@*)(anr=*" + GeneralSearchTerm + "*))";

                        break;
                    case ActiveDirectoryObjectType.User:
                        LdapFilter = "(&(objectCategory=person)(objectClass=user))";
                        if (EnabledOnly == true)
                        {
                            //LdapFilter = "(&(objectCategory=person)(objectClass=user)(!userAccountControl:1.2.840.113556.1.4.803:=2))";
                        }
                        else if (DisabledOnly == true)
                        {
                            //LdapFilter = "(&(objectCategory=person)(objectClass=user)(userAccountControl:1.2.840.113556.1.4.803:=2))";

                        }
                        if (GeneralSearchTerm != null)
                            FilterQuery = "(|(samaccountname=*" + GeneralSearchTerm + "*)(givenname=*" + GeneralSearchTerm + "*)(sn=*" + GeneralSearchTerm + "*)(displayName=*" + GeneralSearchTerm + "*)(anr=*" + GeneralSearchTerm + "*)(mail=" + GeneralSearchTerm + "*@*)(anr=*" + GeneralSearchTerm + "*))";


                        break;
                    case ActiveDirectoryObjectType.Contact:
                        LdapFilter = "(&(objectCategory=person)(objectClass=contact))";

                        if (GeneralSearchTerm != null)
                            FilterQuery = "(|(givenname=*" + GeneralSearchTerm + "*)(sn=*" + GeneralSearchTerm + "*)(displayName=*" + GeneralSearchTerm + "*)(anr=*" + GeneralSearchTerm + "*)(mail=" + GeneralSearchTerm + "*@*)(anr=*" + GeneralSearchTerm + "*))";


                        break;
                    case ActiveDirectoryObjectType.Computer:
                        LdapFilter = "(&(objectCategory=computer))";
                        if (EnabledOnly == true)
                        {
                            //LdapFilter = "(&(objectCategory=computer)(!userAccountControl:1.2.840.113556.1.4.803:=2))";
                        }
                        if (GeneralSearchTerm != null)
                            FilterQuery = "(|(samaccountname=*" + GeneralSearchTerm + "*)(anr=*" + GeneralSearchTerm + "*)(distinguishedName=*" + GeneralSearchTerm + "*)(anr=*" + GeneralSearchTerm + "*))";

                        break;
                    case ActiveDirectoryObjectType.BitLocker:
                        LdapFilter = "(&(objectCategory=msFVE-RecoveryInformation))";
                        if (GeneralSearchTerm != null)

                            LdapFilter = $"(name=*{GeneralSearchTerm}*)";

                        break;
                    case ActiveDirectoryObjectType.OU:
                        // searcher.VirtualListView = null;
                        LdapFilter = "(&(objectCategory=organizationalUnit))";
                        if (GeneralSearchTerm != null)
                            FilterQuery = "(|(distinguishedName=" + GeneralSearchTerm + ")(ou=*" + GeneralSearchTerm + "*)(name=*" + GeneralSearchTerm + "*)(displayName=*" + GeneralSearchTerm + "*)(cn=*" + GeneralSearchTerm + "*)(anr=*" + GeneralSearchTerm + "*))";

                        break;
                }




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
                    if (Fields.LastLogonTime != null)
                        FilterQuery += $"(lastLogonTimestamp<={Fields.LastLogonTime})(!(lastLogonTimestamp=0))";
                    if (Fields.ExpireTime != null)
                        FilterQuery += $"(accountExpires<={Fields.ExpireTime.Value.ToFileTimeUtc().ToString()})(!(accountExpires=0))";
                    if (Fields.LockoutTime != null)
                        FilterQuery += $"(lockoutTime>={Fields.LockoutTime})";
                    if (!Fields.DN.IsNullOrEmpty())
                        FilterQuery += $"(distinguishedName={Fields.DN})";
                    if (!Fields.MemberOf.IsNullOrEmpty())
                        FilterQuery += $"(memberOf=*{Fields.DN})*";
                    if (!Fields.SID.IsNullOrEmpty())
                        FilterQuery += $"(objectSid={Fields.SID})";
                    if (Fields.GUID != null)
                        FilterQuery += $"(objectGUID={Fields.GUID.ToHexADString()})";
                    if (Fields.NestedMemberOf != null)
                        FilterQuery += $"(memberOf:1.2.840.113556.1.4.1941:={Fields.NestedMemberOf.DN})";
                    if (Fields.BitLockerRecoveryId != null)
                        FilterQuery += $"(name=*{Fields.BitLockerRecoveryId}*)";
                    if (Fields.PasswordLastSet != null)
                        FilterQuery += $"(pwdLastSet>={Fields.PasswordLastSet.Value.ToFileTimeUtc().ToString()})";

                    if (FieldValues.Count > 0)
                    {
                        FilterQuery = "";
                        foreach (var field in FieldValues)
                        {
                            var op = "=";
                            var searchValue = "";
                            if (field.Value is DateTime dateTimeValue)
                            {
                                searchValue = dateTimeValue.ToFileTimeUtc().ToString();
                            }
                            else if (field.Value is string strValue)
                            {
                                switch (field.Operator)
                                {
                                    case ActiveDirectoryFieldOperator.EqualTo:
                                        searchValue = $"{field.Value}";
                                        break;
                                    case ActiveDirectoryFieldOperator.StartsWith:
                                        searchValue = $"{field.Value}*";

                                        break;
                                    case ActiveDirectoryFieldOperator.EndsWith:
                                        searchValue = $"*{field.Value}";

                                        break;
                                    case ActiveDirectoryFieldOperator.Contains:
                                        searchValue = $"*{field.Value}*";


                                        break;
                                }
                            }
                            switch (field.Operator)
                            {
                                case ActiveDirectoryFieldOperator.HistoricalTimeFrame:
                                    op = ">=";
                                    if (field.Value is TimeSpan timeSpan2)
                                        searchValue = DateTime.Now.Subtract(timeSpan2).ToFileTimeUtc().ToString();
                                    break;
                                case ActiveDirectoryFieldOperator.FutureTimeFrame:
                                    op = "<=";
                                    if (field.Value is TimeSpan timeSpan3)
                                        searchValue = DateTime.Now.Add(timeSpan3).ToFileTimeUtc().ToString();
                                    break;
                                case ActiveDirectoryFieldOperator.BeforeNow:
                                    op = "<=";
                                    searchValue = DateTime.Now.ToFileTimeUtc().ToString();

                                    break;
                                case ActiveDirectoryFieldOperator.AfterNow:
                                    op = ">=";
                                    searchValue = DateTime.Now.ToFileTimeUtc().ToString();
                                    break;
                                case ActiveDirectoryFieldOperator.Boolean:
                                    break;
                            }
                            if (!searchValue.IsNullOrEmpty())
                            {
                                var negateChar = field.Negate ? "!" : "";
                                FilterQuery += $"({negateChar}{field.Field.FieldName}{op}{searchValue})";
                                if (field.Field.FieldType == ActiveDirectoryFieldType.Date
                                    || field.Field.FieldType == ActiveDirectoryFieldType.FileTime)
                                {
                                    if (field.Operator == ActiveDirectoryFieldOperator.FutureTimeFrame
                                        || field.Operator == ActiveDirectoryFieldOperator.HistoricalTimeFrame)
                                    {
                                        var op2 = field.Operator == ActiveDirectoryFieldOperator.FutureTimeFrame ? ">=" : "<=";

                                        FilterQuery += $"({field.Field.FieldName}{op2}{DateTime.Now.ToFileTimeUtc().ToString()})";


                                    }
                                    FilterQuery += $"(!({field.Field.FieldName}=0))";
                                    FilterQuery += $"(!({field.Field.FieldName}=9223372036854775807))";
                                }

                            }
                        }
                    }

                }

                if (!FilterQuery.IsNullOrEmpty() && ExactMatch)
                {
                    FilterQuery = FilterQuery.Replace("*", "");

                    // Regex pattern:
                    // \\(      -> Match the opening parenthesis literally (needs escaping)
                    // anr=    -> Match "anr=" literally
                    // .*?     -> Match any character (except newline) zero or more times,
                    //           non-greedily (important to stop at the first closing parenthesis)
                    // \\)      -> Match the closing parenthesis literally (needs escaping)
                    // RegexOptions.IgnoreCase -> Make the matching case-insensitive (e.g., matches (Anr=...))
                    string pattern = @"\(anr=.*?\)"; // Using @ verbatim string simplifies escaping

                    // Replace the matched pattern (the entire anr section) with an empty string
                    FilterQuery = Regex.Replace(FilterQuery, pattern, string.Empty, RegexOptions.IgnoreCase);

                }


                if (cancellationToken?.IsCancellationRequested == true)
                    return new();

                // Construct a search request for the specific entry and attribute
                SearchRequest searchRequest = new SearchRequest(
                    SearchRoot.DN, // The DN of the search base
                    LdapFilter, // A filter 
                    SearchScope,
                    "distinguishedName"        // Specify only the attribute you want
                );
                PrepareSearcher(searchRequest);
                if (cancellationToken?.IsCancellationRequested == true)
                    return new();

                using (var connection = SecureLdapConnector.Connect(_currentUserActiveDirectoryContext.ConnectionSettings))
                {
                    if (connection == null) { 
                        return new List<TInterface>();
                    }
                    PerformSearch<TObject, TInterface>(connection, searchRequest, PageSize);

                }
                if (cancellationToken?.IsCancellationRequested == true)
                    return new();

                SearchState = SearchState.Completed;



                if (cancellationToken?.IsCancellationRequested == true)
                    return new();

                OnSearchCompleted?.Invoke();
                stopwatch.Stop();


                return Results.Cast<TInterface>().ToList();


            }
            catch (COMException ex)
            {
                Loggers.ActiveDirectoryLogger.Information("Directory Entry failed to connect {@Error}", ex);
            }
            catch (Exception ex)
            {
                Loggers.ActiveDirectoryLogger.Error("Directory Entry failed to connect {@Error}", ex);
            }

            SearchState = SearchState.Completed;

            OnSearchCompleted?.Invoke();
            stopwatch.Stop();

            return new List<TInterface>();


        }

        private void InitializeSearch()
        {
            stopwatch.Start();
            SearchState = SearchState.Started;
            OnSearchStarted?.Invoke();
            cancellationToken = new();
            Results.Clear();
        }

        private void PerformSearch<TObject, TInterface>(AppLdapConnection searcher, SearchRequest searchRequest, int pageSize) where TObject : IDirectoryEntryAdapter, TInterface, new()
        {
            //SearchResponse? lastResults = (SearchResponse)searcher.SendRequest(searchRequest);
            //AddResults<TObject, TInterface>(lastResults);
            //return;
            // 1. Create the page result request control, specifying the page size.
            var pageRequestControl = new PageResultRequestControl(pageSize);

            // Add the control to the SearchRequest's controls collection.
            searchRequest.Controls.Add(pageRequestControl);
     
            do
            {
                // Check for cancellation before each page request.
                if (cancellationToken?.IsCancellationRequested == true) break;

                // 2. Send the request and get a single page of results.
                SearchResponse searchResponse = (SearchResponse)searcher.SendRequest(searchRequest);
       
                // Find the page response control returned by the server.
                PageResultResponseControl? pageResponseControl = searchResponse.Controls
                    .OfType<PageResultResponseControl>()
                    .FirstOrDefault();

                // Add the retrieved entries to your results collection.
                AddResults<TObject, TInterface>(searchResponse);

                // 3. Check if the server sent back a 'cookie'.
                // An empty cookie means this is the last page of results.
                if (pageResponseControl == null || pageResponseControl.Cookie.Length == 0 || Results.Count >= MaxResults)
                {
                    break; // Exit the loop if there are no more pages.
                }

                // 4. Update the request control with the new cookie for the next iteration.
                pageRequestControl.Cookie = pageResponseControl.Cookie;

            } while (true); // The loop is controlled by the break statement inside.




            //bool moreResults = true;
            //SearchState = SearchState.Collecting;
            //SearchResultCollection lastResults;
            //try
            //{
            //    if (cancellationToken?.IsCancellationRequested == true) return;

            //    lastResults = searcher.FindAll();
            //    if (cancellationToken?.IsCancellationRequested == true) return;

            //    var count = lastResults.Count;
            //}
            //catch
            //{
            //    searcher.VirtualListView = null;
            //    lastResults = searcher.FindAll();
            //}

            //AddResults<TObject, TInterface>(lastResults);

            //if (ObjectTypeFilter != ActiveDirectoryObjectType.OU)
            //{
            //    var approxTotal = searcher.VirtualListView?.ApproximateTotal;
            //    var progress = 0;
            //    if (approxTotal != null && approxTotal > 0)
            //        progress = _searchResults.Count / approxTotal.Value;
            //}
            //if (lastResults.Count < pageSize)
            //    moreResults = false;

            //while (moreResults && cancellationToken?.IsCancellationRequested != true && searcher.VirtualListView != null)
            //{
            //    if (searcher.VirtualListView != null)
            //        searcher.VirtualListView.Offset += pageSize;
            //    //else
            //    //    throw new ApplicationException("The searcher lost it's VirtualListView in the middle of searching!");
            //    lastResults = searcher.FindAll();
            //    AddResults<TObject, TInterface>(lastResults);
            //    if (searcher.VirtualListView == null || lastResults.Count < pageSize)
            //        moreResults = false;

            //}


        }

        private void PrepareSearcher(SearchRequest searcher)
        {
            if (!SearchDeleted)
            {
                searcher.Attributes.Add(ActiveDirectoryFields.SAMAccountName.FieldName);
                searcher.Attributes.Add(ActiveDirectoryFields.DistinguishedName.FieldName);
                searcher.Attributes.Add(ActiveDirectoryFields.ObjectSID.FieldName);
                searcher.Attributes.Add(ActiveDirectoryFields.DisplayName.FieldName);
                searcher.Attributes.Add(ActiveDirectoryFields.Name.FieldName);
                searcher.Attributes.Add(ActiveDirectoryFields.LastLogonTimestamp.FieldName);
                searcher.Attributes.Add("userAccountControl");
                searcher.Attributes.Add("lockouttime");
                searcher.Attributes.Add("objectclass");
                searcher.Attributes.Add("isdeleted");
                searcher.Attributes.Add("whencreated");
                searcher.Attributes.Add("objectguid");
                searcher.Attributes.Add(ActiveDirectoryFields.CanonicalName.FieldName);
            }
            else
            {
                searcher.Attributes.Add("*");

                searcher.Controls.Add(new ShowDeletedControl());
            }


            //searcher.Asynchronous = true;
            searcher.SizeLimit = MaxResults;
            LdapFilter = LdapFilter?.Substring(0, LdapFilter.Length - 1) + FilterQuery + ")";
            searcher.Filter = LdapFilter;
        }
        /// <summary>
        /// Cancels the current search if still running
        /// </summary>
        public void Cancel()
        {
            Results.Clear();

            cancellationToken = new CancellationToken(true);

        }



        private void AddResults<T, I>(SearchResponse lastResults) where T : I, IDirectoryEntryAdapter, new()
        {
            List<IDirectoryEntryAdapter> last;
            if (_currentUserActiveDirectoryContext != null)
            {
                last = lastResults.Entries.Encapsulate(_currentUserActiveDirectoryContext);

            }
            else
            {
                last = lastResults.Entries.Encapsulate(ActiveDirectoryContext.SystemInstance);
            }
            if (EnabledOnly == true)
            {
                Results.AddRange(last.Where(l => l is not IAccountDirectoryAdapter || (l as IAccountDirectoryAdapter).Enabled));

            }
            else if (DisabledOnly)
            {
                Results.AddRange(last.Where(l=>l is not IAccountDirectoryAdapter || (l as IAccountDirectoryAdapter).Disabled));
            }
            else
            {
                Results.AddRange(last);
            }

            ResultsCollected?.Invoke(last);

        }


    }

    public class ADFieldValue
    {

        public IActiveDirectoryField? Field
        {
            get
            {
                if (DefaultField != null)
                {
                    return DefaultField;
                }
                if (CustomField != null)
                {
                    return CustomField;
                }
                return null;
            }
            set
            {
                if (value is CustomActiveDirectoryField field)
                {
                    DefaultField = null;
                    CustomField = field;
                }
                else if (value is ActiveDirectoryField field2)
                {
                    CustomField = null;
                    DefaultField = field2;
                }
            }
        }
        protected ActiveDirectoryField DefaultField { get; set; }
        protected CustomActiveDirectoryField CustomField { get; set; }
        public ActiveDirectoryFieldOperator Operator { get; set; }
        public bool Negate { get; set; }
        public object? Value { get; set; }
    }
}
