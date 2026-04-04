using BLAZAM.ActiveDirectory.Adapters;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.ActiveDirectory.Searchers;
using BLAZAM.Common.Data;
using BLAZAM.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using MudBlazor;

namespace BLAZAM.Gui.UI.Inputs
{
    public partial class ADAutoComplete : AutoCompleteComponentBase
    {

        [Parameter]
        public Func<object> Validation { get; set; }
        [Parameter]
        public string? Label { get; set; }
        [Parameter]
        public string? Style { get; set; }
        [Parameter]
        public Variant Variant { get; set; }
        [Parameter]
        public string? Class { get; set; }
        [Parameter]
        public string? AdornmentText { get; set; }
        [Parameter]
        public string AdornmentIcon { get; set; } = Icons.Material.Filled.Search;
        [Parameter]
        public Adornment Adornment { get; set; } = Adornment.Start;
        [Parameter]
        public Origin TransformOrigin { get; set; } = Origin.TopCenter;

        [Parameter]
        public Origin AnchorOrigin { get; set; } = Origin.BottomCenter;

        /// <summary>
        /// Disables this autocomplete input
        /// </summary>
        [Parameter]
        public bool Disabled { get; set; }

        /// <summary>
        /// Automatically bring the text box into focus on load
        /// </summary>
        [Parameter]
        public bool AutoFocus { get; set; }

        /// <summary>
        /// The maximum number of results to show in the autocomplete suggestion list
        /// </summary>
        /// <remarks>
        /// Defaults to 10
        /// </remarks>
        [Parameter]
        public int MaxResults { get; set; } = 500;


        [Parameter]
        public bool AllowCustomInput { get; set; }

        private MudAutocomplete<IDirectoryEntryAdapter>? AutoComplete { get; set; }

        private readonly List<CancellationTokenSource> _cancellationList = new();

        [Parameter]
        public Func<IDirectoryEntryAdapter, bool> CustomResultsFilter { get; set; }

        [Parameter]
        public Func<IDirectoryEntryAdapter, bool>? CustomDisableFilter { get; set; }


        [CascadingParameter]
        public SearchService SearchParameters { get; set; }

        IEnumerable<IDirectoryEntryAdapter> _searchResults = new List<IDirectoryEntryAdapter>();
        bool _searchDisabled;

        /// <summary>
        /// Indicates whether to search for disabled Users/Computers
        /// </summary>
        [Parameter]
        public bool SearchDisabled
        {
            get => _searchDisabled;
            set
            {
                if (_searchDisabled == value)
                    return;
                _searchDisabled = value;
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            _ = CancelExistingTokens();
        }

        [Parameter]
        public ActiveDirectoryObjectType? SearchObjectType { get; set; }


        [Parameter]
        public IEnumerable<IDirectoryEntryAdapter> SearchResults
        {
            get => _searchResults;
            set
            {
                if (_searchResults == value)
                    return;
                _searchResults = value;
                SearchResultsChanged.InvokeAsync(value);
            }
        }


        [Parameter]
        public EventCallback<IEnumerable<IDirectoryEntryAdapter>> SearchResultsChanged { get; set; }


        [Parameter]
        public EventCallback OnAdornmentClick { get; set; }

        private IDirectoryEntryAdapter? _selectedResult;
        [Parameter]
        public IDirectoryEntryAdapter? SelectedResult
        {
            get => _selectedResult;
            set
            {
                if (_selectedResult == value)
                    return;

                SearchResults = new List<IDirectoryEntryAdapter>
                {
                    value
                };
                _selectedResult = value;

                SearchService.SearchTerm = _selectedResult != null ? _selectedResult.CanonicalName : "";
                _ = CancelExistingTokens();
                SelectedResultChanged.InvokeAsync(value);
            }
        }

        [Parameter]
        public EventCallback<IDirectoryEntryAdapter?> SelectedResultChanged { get; set; }



        private string _selectedSid;
        [Parameter]
        public string SelectedSid
        {
            get => _selectedSid;
            set
            {
                if (_selectedSid == value)
                    return;


                _selectedSid = value;

                SelectedSidChanged.InvokeAsync(value);
            }
        }


        [Parameter]
        public EventCallback<string> SelectedSidChanged { get; set; }


        private Guid? _selectedGuid;
        [Parameter]
        public Guid? SelectedGuid
        {
            get => _selectedGuid;
            set
            {
                if (_selectedGuid == value)
                    return;


                _selectedGuid = value;

                SelectedGuidChanged.InvokeAsync(value);
            }
        }


        [Parameter]
        public EventCallback<Guid?> SelectedGuidChanged { get; set; }

        /// <summary>
        /// Clears the results and entered search term
        /// </summary>
        public void Clear()
        {
            SearchTerm = String.Empty;
            SearchResults = new List<IDirectoryEntryAdapter>();
        }




        private async Task CancelExistingTokens()
        {
            foreach (var source in _cancellationList)
            {
                await source.CancelAsync();
                source.Dispose();
            }
            _cancellationList.Clear();
        }


        private async Task<IEnumerable<IDirectoryEntryAdapter>> GetResults(string searchText, CancellationToken token)
        {
            await CancelExistingTokens();
            var newSource = CancellationTokenSource.CreateLinkedTokenSource(token);
            var newToken = newSource.Token;
            _cancellationList.Add(newSource);


            if (string.IsNullOrEmpty(searchText))
            {
                return Enumerable.Empty<IDirectoryEntryAdapter>();
            }

            if (newToken.IsCancellationRequested || string.IsNullOrEmpty(searchText))
            {
                return Enumerable.Empty<IDirectoryEntryAdapter>();
            }

            SearchResults = new List<IDirectoryEntryAdapter>();
            var search = new ADSearch(Directory)
            {
                ObjectTypeFilter = SearchObjectType,
                GeneralSearchTerm = searchText,
                MaxResults = MaxResults,
                EnabledOnly = !SearchDisabled
            };


            var tempResults = (await search.SearchAsync<DirectoryEntryAdapter, IDirectoryEntryAdapter>(newToken))
                .Where(r => r.CanRead);

            if (newToken.IsCancellationRequested)
            {
                return Enumerable.Empty<IDirectoryEntryAdapter>();
            }

            IEnumerable<IDirectoryEntryAdapter> filteredResults = tempResults;
            if (CustomResultsFilter != null)
            {
                filteredResults = tempResults.Where(result => CustomResultsFilter.Invoke(result));
            }

            if (newToken.IsCancellationRequested)
            {
                return Enumerable.Empty<IDirectoryEntryAdapter>();
            }

            SearchResults = filteredResults.ToList();
            return SearchResults;
        }
        private void AddResults(IEnumerable<IDirectoryEntryAdapter> resultsPage)
        {
            foreach (var result in resultsPage)
            {
                SearchResults = SearchResults.Append(result);
            }
            _ = StateHasChangedAsync();
        }
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            if (!SelectedSid.IsNullOrEmpty())
            {
                _ = Task.Run(async () =>
                {
                    if (SelectedSid != null)
                    {
                        var sidMatch = Directory.FindGlobalEntryBySid(SelectedSid.ToSidByteArray());
                        await InvokeAsync(() => { SelectedResult = sidMatch; SearchTerm = sidMatch.CanonicalName; });
                    }
                    else if (SelectedGuid != Guid.Empty)
                    {
                        var guidMatch = Directory.FindGlobalEntryByGuid(SelectedGuid.Value.ToByteArray());
                        await InvokeAsync(() => { SelectedResult = guidMatch; SearchTerm = guidMatch.CanonicalName; });
                    }
                    _ = StateHasChangedAsync();
                });
            }
        }


    }
}