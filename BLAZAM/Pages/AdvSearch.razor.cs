using BLAZAM.ActiveDirectory.Searchers;
using BLAZAM.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BLAZAM.Pages
{
    /// <summary>
    /// Represents a search component that facilitates searching for directory entries based on various parameters and
    /// filters.
    /// </summary>
    /// <remarks>This component integrates with the <see cref="SearchService"/> to manage search parameters
    /// and performs searches using the <see cref="ADSearch"/> class. It supports initializing search terms from the
    /// URI, handling cascading parameters, and managing search results.</remarks>
    public partial class AdvSearch : SearchPageBase
    {
        /// <summary>
        /// Gets or sets the search GUID used to identify the current search operation. This parameter is used to determine
        /// if it should repeat a previous search, currently unimplemented.
        /// </summary>
        [Parameter]
        public string? SearchGuid { get; set; }

        private string? _previousSeachGuid;

        protected override async Task OnParametersSetAsync()
        {
            if (_previousSeachGuid != SearchGuid)
            {
                _previousSeachGuid = SearchGuid;
                await PerformSearch();
            }
        }
        protected override async Task PerformSearch()
        {
            LoadingData = true;
            if (SearchService.Filters != null && SearchService.Filters.Count > 0)
            {
                Searcher.Results = await SearchService.Filters.GetFilteredEntries(SearchService.SeachObjectType, DbFactory, Directory);
            }
            LoadingData = false;
        }
    }
}