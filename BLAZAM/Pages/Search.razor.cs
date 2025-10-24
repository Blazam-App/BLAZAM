// Import necessary namespaces for various functionalities
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
    /// Represents a search component that facilitates searching for directory entries based on various parameters and
    /// filters.
    /// </summary>
    /// <remarks>This component integrates with the <see cref="SearchService"/> to manage search parameters
    /// and performs searches using the <see cref="ADSearch"/> class. It supports initializing search terms from the
    /// URI, handling cascading parameters, and managing search results.</remarks>
    public partial class Search : AppComponentBase
    {
        /// <summary>
        /// Gets or sets the search parameters used to configure the search operation.
        /// </summary>
        [CascadingParameter]
        public SearchService? SearchParameters { get; set; }

        private readonly string ModelsTypeName = "Search";

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
        /// Asynchronously handles updates to component parameters and performs a search operation  if the search term
        /// has changed.
        /// </summary>
        /// <remarks>This method is invoked whenever parameters are set for the component. If the search
        /// term  has changed, it cancels any ongoing search, decodes the new search term, and initiates a  new search
        /// operation. The <see cref="AppComponentBase.LoadingData"/> property is updated to reflect the  loading state during the search
        /// process.</remarks>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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
        /// Gets or sets the directory searcher used to query Active Directory.
        /// </summary>
        public ADSearch? Searcher { get; set; }

        /// <summary>
        /// Gets or sets the cascading parameter for the <see cref="MainLayout"/> component.
        /// </summary>
        /// <remarks>This property is typically used to share the <see cref="MainLayout"/> instance with
        /// child components in a Blazor application. Ensure that the property is properly initialized before accessing
        /// it.</remarks>
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
            SearchService.SearchTerm = SearchTermParameter;
            Searcher.GeneralSearchTerm = SearchTermParameter;

            await base.OnInitializedAsync();



            Searcher.OnSearchStarted += (async () =>
            {
                await StateHasChangedAsync();

            });
            Searcher.OnSearchCompleted += (async () =>
            {
                await StateHasChangedAsync();
            });
            Searcher.ResultsCollected += ((batch) =>
                 {
                     results.AddRange(batch.Where(r => r.CanRead));
                 });
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
        protected async Task PerformSearch()
        {
            results.Clear();

            LoadingData = true;
            if (!SearchTermParameter.IsNullOrEmpty() && SearchTermParameter?.Length > 0)
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

            SearchService.SearchTerm = SearchTermParameter;
            Searcher.EnabledOnly = !SearchService.IncludeDisabled;
            Searcher.GeneralSearchTerm = SearchService.SearchTerm;
            Searcher.ObjectTypeFilter = SearchService.SeachObjectType;
            Searcher.ExactMatch = true;

            //Try exact  match search first
            await Searcher.SearchAsync<DirectoryEntryAdapter, IDirectoryEntryAdapter>();
            if (Searcher.Results.Count < 1)
            {
                Searcher.ExactMatch = false;
                results = await Searcher.SearchAsync<DirectoryEntryAdapter, IDirectoryEntryAdapter>();

            }
        }

    }
}