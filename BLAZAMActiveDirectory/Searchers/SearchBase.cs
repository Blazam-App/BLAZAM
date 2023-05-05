using Microsoft.AspNetCore.Components;
namespace BLAZAM.ActiveDirectory.Searchers
{
    public class SearchBase
    {
        protected SearchState searchState = SearchState.Ready;
        /// <summary>
        /// Event fired when all results have been collected, or an error occurred
        /// </summary>
        public AppEvent OnSearchCompleted { get; set; }
        public AppEvent OnSearchStarted { get; set; }
        public TimeSpan SearchTime { get; set; }

        /// <summary>
        /// Indicates the current state of this search
        /// </summary>
        public SearchState SearchState
        {
            get => searchState; set
            {
                if (searchState == value) return;
                searchState = value;
                SearchStateChanged.InvokeAsync(value);
            }
        }
        public EventCallback<SearchState> SearchStateChanged { get; set; }
        /// <summary>
        /// Allows for cancelling the search
        /// </summary>
        protected CancellationToken? cancellationToken { get; set; } = new CancellationToken();
    }
}