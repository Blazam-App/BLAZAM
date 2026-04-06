using BLAZAM.ActiveDirectory.Adapters;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.ActiveDirectory.Searchers;
using BLAZAM.Common.Data;
using BLAZAM.Gui.Layouts;
using BLAZAM.Gui.UI;
using BLAZAM.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Web;

namespace BLAZAM.Pages
{
    /// <summary>
    /// Represents a view component that facilitates searching within a directory structure.
    /// </summary>
    /// <remarks>This class provides functionality for initializing search parameters, performing searches,
    /// and managing search results. It integrates with cascading parameters such as <see cref="SearchService"/>, <see
    /// cref="MainLayout"/>, and <see cref="SearchObjectType"/>  to enable seamless interaction with the application's
    /// state and layout. The search process is managed through the <see cref="ADSearch"/>  object, which handles the
    /// execution of search queries and result collection.</remarks>
    public class SearchPageBase : AppComponentBase
    {
        /// <summary>
        /// Gets or sets the search parameters used to configure the search operation.
        /// </summary>
        [CascadingParameter]
        public SearchService? SearchParameters { get; set; }

        protected readonly string ModelsTypeName = "Search";

        /// <summary>
        /// Gets or sets the icon used to represent the search functionality.
        /// </summary>
        protected string SearchIcon { get; set; } = "";
        private string? _searchTermParameter;
        /// <summary>
        /// The search term that comes from the URI
        /// </summary>
        [Parameter]
        public virtual string? SearchTermParameter { get; set; }

        /// <summary>
        /// Asynchronously handles updates to component parameters and triggers a search operation  if the search term
        /// has changed.
        /// </summary>
        /// <remarks>This method decodes the search term parameter and compares it to the previously
        /// stored value.  If the search term has changed, it cancels any ongoing search, updates the internal state, 
        /// and initiates a new search operation. If the search term is empty, the loading state is reset.</remarks>
        /// <returns></returns>
        protected override async Task OnParametersSetAsync()
        {
            var decodedSearchTerm = HttpUtility.UrlDecode(SearchTermParameter);

            if (_searchTermParameter == decodedSearchTerm)
            {
                return;
            }

            LoadingData = true;
            Searcher?.Cancel();
            _searchTermParameter = decodedSearchTerm;

            await PerformSearch();

            if (_searchTermParameter.IsNullOrEmpty())
            {
                LoadingData = false;
            }
        }

        /// <summary>
        /// Gets or sets the directory searcher used to perform Active Directory queries.
        /// </summary>
        /// <remarks>This property allows customization of the searcher used for querying Active
        /// Directory.  Ensure the searcher is properly configured before performing any operations that depend on
        /// it.</remarks>
        protected ADSearch? Searcher { get; set; }

        /// <summary>
        /// Gets or sets the cascading parameter for the main layout of the application.
        /// </summary>
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

            Searcher.OnSearchStarted += OnSearchUpdated;
            Searcher.OnSearchCompleted += OnSearchUpdated;
            Searcher.ResultsCollected += AddResults;
        }

        /// <summary>
        /// Releases the resources used by the current instance and unsubscribes from all event handlers.
        /// </summary>
        /// <remarks>This method ensures that all event subscriptions associated with the <see
        /// cref="Searcher"/> object are removed to prevent memory leaks. It also calls the base class's <see
        /// cref="Dispose"/> method to release any additional resources.</remarks>
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

        /// <summary>
        /// Gets or sets the collection of directory entry adapters.
        /// </summary>
        protected virtual List<IDirectoryEntryAdapter> results { get; set; } = [];


        /// <summary>
        /// Called internally to start a search
        /// </summary>
        /// <remarks>
        /// This method in turn calls <see cref="InvokeSearch"/>
        /// if the <see cref="SearchTermParameter"/> is not
        /// null or empty.
        /// </remarks>
        /// <returns></returns>
        protected virtual async Task PerformSearch()
        {
            results.Clear();

            LoadingData = true;
            if (!_searchTermParameter.IsNullOrEmpty() && _searchTermParameter?.Length > 0)
            {
                await InvokeSearch();
            }
            else
            {
                Searcher?.Results.Clear();
            }

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
            {
                Searcher = new ADSearch(Directory);
            }
            else
            {
                Searcher.Cancel();
            }

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