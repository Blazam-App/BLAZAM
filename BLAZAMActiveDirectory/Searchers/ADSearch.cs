using BLAZAM.ActiveDirectory.Adapters;
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
        public DirectoryEntry SearchRoot { get; set; }
        /// <summary>
        /// Indicates whether the search is single level or recursive default is recursive
        /// </summary>
        public SearchScope SearchScope { get; set; } = SearchScope.Subtree;

        /// <summary>
        /// The realtime results of this search. 
        /// <para>Check <see cref="SearchState"/>
        /// or listen to <see cref="OnSearchCompleted"/>
        /// to confirm search is completed and no more results are coming.</para>
        /// </summary>
        public AppDelegate<IEnumerable<IDirectoryEntryAdapter>> ResultsCollected { get; set; }

        private int PageSize = 40;

        public ActiveDirectoryObjectType? ObjectTypeFilter { get; set; }
        public bool? EnabledOnly { get; set; }
        public int MaxResults { get; set; } = 500;
        private List<SearchResult> _searchResults = new();

        public List<IDirectoryEntryAdapter> Results { get; set; } = new();
        public string LdapQuery { get; private set; }
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
                var pageOffset = 1;

                searcher = new DirectorySearcher(SearchRoot)
                {
                    VirtualListView = new DirectoryVirtualListView(0, PageSize - 1, pageOffset),
                    PageSize = PageSize,
                    Sort = new SortOption(ActiveDirectoryFields.CanonicalName.FieldName, SortDirection.Ascending),
                    SearchScope = SearchScope,
                    SizeLimit = MaxResults,
                    Filter = "(&(|(&(objectClass=user)(!userAccountControl:1.2.840.113556.1.4.803:=2))(objectClass=group)(objectClass=contact)(&(objectCategory=computer)(!userAccountControl:1.2.840.113556.1.4.803:=2))(objectClass=organizationalUnit)(objectClass=printQueue)))"
                };
                if (EnabledOnly == false)
                {
                    searcher.Filter = searcher.Filter.Replace("(!userAccountControl:1.2.840.113556.1.4.803:=2)", "");
                }
                else if (DisabledOnly == true)
                {
                    searcher.Filter = searcher.Filter.Replace("(!userAccountControl:1.2.840.113556.1.4.803:=2)", "(userAccountControl:1.2.840.113556.1.4.803:=2)");

                }
                if (SearchDeleted)
                    searcher.Filter = searcher.Filter.Substring(0, searcher.Filter.Length - 1) + "(isDeleted=TRUE)" + ")";

                switch (ObjectTypeFilter)
                {
                    case ActiveDirectoryObjectType.All:
                    case null:
                        if (GeneralSearchTerm != null)
                            FilterQuery = "(|(samaccountname=*" + GeneralSearchTerm + "*)(cn=*" + GeneralSearchTerm + "*)(distinguishedName=" + GeneralSearchTerm + ")(givenname=*" + GeneralSearchTerm + "*)(sn=*" + GeneralSearchTerm + "*)(displayName=*" + GeneralSearchTerm + "*)(name=*" + GeneralSearchTerm + "*)(mail=*" + GeneralSearchTerm + "*@*)(anr=*" + GeneralSearchTerm + "*))";
                        break;
                    case ActiveDirectoryObjectType.Printer:
                        searcher.Filter = "(&(objectClass=printQueue))";
                        if (GeneralSearchTerm != null)
                            FilterQuery = "(|(samaccountname=*" + GeneralSearchTerm + "*)(displayName=*" + GeneralSearchTerm + "*)(name=*" + GeneralSearchTerm + "*)(cn=*" + GeneralSearchTerm + "*)(anr=*" + GeneralSearchTerm + "*))";

                        break;
                    case ActiveDirectoryObjectType.Group:
                        searcher.Filter = "(&(objectCategory=group)(objectClass=group))";
                        if (GeneralSearchTerm != null)
                            FilterQuery = "(|(samaccountname=*" + GeneralSearchTerm + "*)(displayName=*" + GeneralSearchTerm + "*)(name=*" + GeneralSearchTerm + "*)(cn=*" + GeneralSearchTerm + "*)(mail=" + GeneralSearchTerm + "*@*)(anr=*" + GeneralSearchTerm + "*))";

                        break;
                    case ActiveDirectoryObjectType.User:
                        searcher.Filter = "(&(objectCategory=person)(objectClass=user))";
                        if (EnabledOnly == true)
                        {
                            searcher.Filter = "(&(objectCategory=person)(objectClass=user)(!userAccountControl:1.2.840.113556.1.4.803:=2))";
                        }
                        else if (DisabledOnly == true)
                        {
                            searcher.Filter = "(&(objectCategory=person)(objectClass=user)(userAccountControl:1.2.840.113556.1.4.803:=2))";

                        }
                        if (GeneralSearchTerm != null)
                            FilterQuery = "(|(samaccountname=*" + GeneralSearchTerm + "*)(givenname=*" + GeneralSearchTerm + "*)(sn=*" + GeneralSearchTerm + "*)(displayName=*" + GeneralSearchTerm + "*)(anr=*" + GeneralSearchTerm + "*)(mail=" + GeneralSearchTerm + "*@*)(anr=*" + GeneralSearchTerm + "*))";


                        break;
                    case ActiveDirectoryObjectType.Contact:
                        searcher.Filter = "(&(objectCategory=person)(objectClass=contact))";

                        if (GeneralSearchTerm != null)
                            FilterQuery = "(|(givenname=*" + GeneralSearchTerm + "*)(sn=*" + GeneralSearchTerm + "*)(displayName=*" + GeneralSearchTerm + "*)(anr=*" + GeneralSearchTerm + "*)(mail=" + GeneralSearchTerm + "*@*)(anr=*" + GeneralSearchTerm + "*))";


                        break;
                    case ActiveDirectoryObjectType.Computer:
                        searcher.Filter = "(&(objectCategory=computer))";
                        if (EnabledOnly == true)
                        {
                            searcher.Filter = "(&(objectCategory=computer)(!userAccountControl:1.2.840.113556.1.4.803:=2))";
                        }
                        if (GeneralSearchTerm != null)
                            FilterQuery = "(|(samaccountname=*" + GeneralSearchTerm + "*)(anr=*" + GeneralSearchTerm + "*)(distinguishedName=*" + GeneralSearchTerm + "*)(anr=*" + GeneralSearchTerm + "*))";

                        break;
                    case ActiveDirectoryObjectType.BitLocker:
                        searcher.Filter = "(&(objectCategory=msFVE-RecoveryInformation))";
                        if (GeneralSearchTerm != null)

                            searcher.Filter = $"(name=*{GeneralSearchTerm}*)";

                        break;
                    case ActiveDirectoryObjectType.OU:
                        searcher.VirtualListView = null;
                        searcher.Filter = "(&(objectCategory=organizationalUnit))";
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
                                    if(field.Operator== ActiveDirectoryFieldOperator.FutureTimeFrame 
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

                PrepareSearcher(searcher);
                if (cancellationToken?.IsCancellationRequested == true)
                    return new();


                PerformSearch<TObject, TInterface>(searcher, PageSize);

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

        private void PerformSearch<TObject, TInterface>(DirectorySearcher searcher, int pageSize) where TObject : IDirectoryEntryAdapter, TInterface, new()
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

            AddResults<TObject, TInterface>(lastResults);

            if (ObjectTypeFilter != ActiveDirectoryObjectType.OU)
            {
                var approxTotal = searcher.VirtualListView?.ApproximateTotal;
                var progress = 0;
                if (approxTotal != null && approxTotal > 0)
                    progress = _searchResults.Count / approxTotal.Value;
            }
            if (lastResults.Count < pageSize)
                moreResults = false;

            while (moreResults && cancellationToken?.IsCancellationRequested != true && searcher.VirtualListView != null)
            {
                if (searcher.VirtualListView != null)
                    searcher.VirtualListView.Offset += pageSize;
                
                lastResults = searcher.FindAll();
                AddResults<TObject, TInterface>(lastResults);
                if (searcher.VirtualListView == null || lastResults.Count < pageSize)
                    moreResults = false;

            }


        }

        private void PrepareSearcher(DirectorySearcher searcher)
        {
            if (!SearchDeleted)
            {
                searcher.PropertiesToLoad.Add(ActiveDirectoryFields.SAMAccountName.FieldName);
                searcher.PropertiesToLoad.Add(ActiveDirectoryFields.DistinguishedName.FieldName);
                searcher.PropertiesToLoad.Add(ActiveDirectoryFields.ObjectSID.FieldName);
                searcher.PropertiesToLoad.Add("objectclass");
                searcher.PropertiesToLoad.Add(ActiveDirectoryFields.CanonicalName.FieldName);
                searcher.PropertiesToLoad.Add("name");
            }
            if (SearchDeleted)
            {
                searcher.Tombstone = true;
                searcher.VirtualListView = new DirectoryVirtualListView(0, PageSize - 1, 1);

            }


            searcher.SizeLimit = MaxResults;
            searcher.Filter = searcher.Filter?.Substring(0, searcher.Filter.Length - 1) + FilterQuery + ")";
            LdapQuery = searcher.Filter;
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
            List<IDirectoryEntryAdapter> last;
            if (_currentUserActiveDirectoryContext != null)
            {
                last = lastResults.Encapsulate(_currentUserActiveDirectoryContext);

            }
            else
            {
                last = lastResults.Encapsulate(ActiveDirectoryContext.SystemInstance);
            }
            Results.AddRange(last);

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
        protected ActiveDirectoryField? DefaultField { get; set; }
        protected CustomActiveDirectoryField? CustomField { get; set; }
        public ActiveDirectoryFieldOperator Operator { get; set; }
        public bool Negate { get; set; }
        public object? Value { get; set; }
    }
}
