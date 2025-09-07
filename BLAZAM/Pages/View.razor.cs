using System.Web;
using BLAZAM.ActiveDirectory.Adapters;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.ActiveDirectory.Searchers;
using BLAZAM.Common.Data;
using BLAZAM.Gui.Layouts;
using BLAZAM.Gui.UI;
using BLAZAM.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BLAZAM.Pages
{
    public partial class View : AppComponentBase
    {

        [CascadingParameter]
        public SearchService? SearchParameters { get; set; }

        readonly string ModelsTypeName = "Search";
        protected string SearchIcon { get; set; } = "";
        string? _searchTermParameter;
        /// <summary>
        /// The search term that comes from the URI
        /// </summary>
        [Parameter]
        public virtual string? SearchTermParameter { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            var decodedSearchTerm = HttpUtility.UrlDecode(SearchTermParameter);

            if (_searchTermParameter == decodedSearchTerm)
                return;

            LoadingData = true;
            Searcher?.Cancel();
            _searchTermParameter = decodedSearchTerm;

            await PerformSearch();

            if (_searchTermParameter.IsNullOrEmpty())
                LoadingData = false;
        }


        public ADSearch? Searcher { get; set; }


        [CascadingParameter]
        public MainLayout? MainLayout { get; set; }

        /// <summary>
        /// The search term that comes from the search text box
        /// </summary>
        [CascadingParameter]
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Standard search page initializer that copies the url search term to the
        /// text search term if it is set.
        /// </summary>
        /// <returns></returns>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            Searcher = new ADSearch(Directory);
            SearchService.SearchTerm = _searchTermParameter;
            Searcher.GeneralSearchTerm = _searchTermParameter;

            await base.OnInitializedAsync();



            Searcher.OnSearchStarted += OnSearchUpdated;
            Searcher.OnSearchCompleted += OnSearchUpdated;
            Searcher.ResultsCollected += AddResults;
        }

        public override void Dispose()
        {
            base.Dispose();
            if (Searcher?.OnSearchStarted != null)
            {
                Searcher.OnSearchStarted -= OnSearchUpdated;
            }
            if (Searcher?.OnSearchCompleted != null)
            {
                Searcher.OnSearchCompleted -= OnSearchUpdated;
            }

            if (Searcher?.ResultsCollected != null)
            {
                Searcher.ResultsCollected -= AddResults;
            }
        }

        private void AddResults(IEnumerable<IDirectoryEntryAdapter> batch)
        {
            results.AddRange(batch.Where(r => r.CanRead));
        }
        private void OnSearchUpdated()
        {
            _ = StateHasChangedAsync();
        }
        /// <summary>
        /// Filter for searching objects of only this type
        /// </summary>
        [CascadingParameter]
        public ActiveDirectoryObjectType? SearchObjectType { get; set; }

        protected virtual List<IDirectoryEntryAdapter> results { get; set; } = new List<IDirectoryEntryAdapter>();


        /// <summary>
        /// Called internally to start a search
        /// </summary>
        /// <remarks>
        /// This method in turn calls <see cref="InvokeSearch"/>
        /// if the <see cref="SearchTermParameter"/> is not
        /// null or empty.
        /// </remarks>
        /// <returns></returns>
        protected async Task PerformSearch()
        {
            results.Clear();

            LoadingData = true;
            if (!_searchTermParameter.IsNullOrEmpty() && _searchTermParameter?.Length > 0)
                await InvokeSearch();
            else
                Searcher?.Results.Clear();
            LoadingData = false;




        }

        /// <summary>
        /// Invokes the actual search function
        /// that processes the current <see cref="SearchService"/>
        /// settings.
        /// </summary>
        /// <returns></returns>
        protected async Task InvokeSearch()
        {
            if (Searcher == null)
                Searcher = new ADSearch(Directory);
            else
                Searcher.Cancel();
            SearchService.SearchTerm = _searchTermParameter;
            Searcher.GeneralSearchTerm = SearchService.SearchTerm;
            if (CurrentUser.State.CanSearchDisabled(ActiveDirectoryObjectType.User)
        || CurrentUser.State.CanSearchDisabled(ActiveDirectoryObjectType.Computer))
            {
                Searcher.EnabledOnly = false;
            }
            Searcher.ExactMatch = true;

            //Try exact  match search first
            await Searcher.SearchAsync<DirectoryEntryAdapter, IDirectoryEntryAdapter>();
            if (Searcher.Results.Count < 1)
            {
                results = await Searcher.SearchAsync<DirectoryEntryAdapter, IDirectoryEntryAdapter>();

            }
        }

    }
}