// Import necessary namespaces for various functionalities
using BLAZAM.ActiveDirectory.Adapters;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.ActiveDirectory.Searchers;
using BLAZAM.Services;

namespace BLAZAM.Pages
{
    public partial class Search : SearchPage
    {
        /// <summary>
        /// Invokes the actual search function
        /// that processes the current <see cref="SearchService"/>
        /// settings.
        /// </summary>
        /// <returns></returns>
        protected override async Task InvokeSearch()
        {
            if (Searcher == null)
                Searcher = new ADSearch(Directory);
            else
                Searcher.Cancel();
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