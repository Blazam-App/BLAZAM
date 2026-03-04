// Import necessary namespaces for various functionalities
using BLAZAM.ActiveDirectory.Adapters;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.ActiveDirectory.Searchers;
using BLAZAM.Common.Data;
using BLAZAM.Gui.Layouts;
using BLAZAM.Gui.UI;
using BLAZAM.Gui.UI.Outputs;
using BLAZAM.Services;
using BLAZAM.Services.Helpers;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Diagnostics;
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
    public partial class AdvSearch : SearchPageBase
    {
        [Parameter]
        public string? seachGuid { get; set; }

        private string? _previousSeachGuid;

        protected override async Task OnParametersSetAsync()
        {
            if (_previousSeachGuid != seachGuid)
            {
                _previousSeachGuid = seachGuid;
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