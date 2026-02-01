// Import necessary namespaces for various functionalities
using System.Web;
using BLAZAM.ActiveDirectory.Adapters;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.ActiveDirectory.Searchers;
using BLAZAM.Common.Data;
using BLAZAM.Gui.Layouts;
// Import necessary namespaces for various functionalities
using BLAZAM.Gui.UI;
using BLAZAM.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BLAZAM.Pages
{
    public class SearchPage : AppComponentBase
    {

        protected readonly string ModelsTypeName = "Search";


        [CascadingParameter]
        public MainLayout? MainLayout { get; set; }

        /// <summary>
        /// The search term that comes from the search text box
        /// </summary>
        [CascadingParameter]
        public string? SearchTerm { get; set; }


        protected ADSearch? Searcher { get; set; }

        [CascadingParameter]
        public SearchService? SearchParameters { get; set; }
        protected string SearchIcon { get; set; } = "";

        protected virtual List<IDirectoryEntryAdapter> results { get; set; } = new List<IDirectoryEntryAdapter>();
        protected string? _searchTerm;
        /// <summary>
        /// The search term that comes from the URI
        /// </summary>
        [Parameter]
        public virtual string? SearchTermParameter { get; set; }

        /// <summary>
        /// Invokes the actual search function
        /// that processes the current <see cref="SearchService"/>
        /// settings.
        /// </summary>
        /// <returns></returns>
        protected virtual async Task InvokeSearch()
        {
            if (Searcher == null)
                Searcher = new ADSearch(Directory);
            else
                Searcher.Cancel();
            SearchService.SearchTerm = SearchTermParameter;
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
                await InvokeSearch();
            else
                Searcher?.Results.Clear();
            LoadingData = false;




        }

        /// <summary>
        /// Standard search page initializer that copies the url search term to the
        /// text search term if it is set.
        /// </summary>
        /// <returns></returns>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await InvokeAsync(StateHasChanged);
            Searcher = new ADSearch(Directory);
            SearchService.SearchTerm = SearchTermParameter;
            Searcher.GeneralSearchTerm = SearchTermParameter;

            LoadingData = false;



            Searcher.OnSearchStarted += (() =>
           {
               InvokeAsync(StateHasChanged);
           });
            Searcher.OnSearchCompleted += (() =>
           {
               InvokeAsync(StateHasChanged);
           });
            Searcher.ResultsCollected += ((batch) =>
                 {
                     results.AddRange(batch.Where(r => r.CanRead));
                 });


        }

        protected override async Task OnParametersSetAsync()
        {

            var decodedSearchTerm = HttpUtility.UrlDecode(SearchTermParameter);

            if (_searchTerm == decodedSearchTerm)
                return;

            LoadingData = true;
            Searcher?.Cancel();
            _searchTerm = decodedSearchTerm;

            await PerformSearch();

            LoadingData = false;
        }
    }
}