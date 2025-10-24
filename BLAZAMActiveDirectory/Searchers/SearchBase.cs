using Microsoft.AspNetCore.Components;
using System.Diagnostics;
namespace BLAZAM.ActiveDirectory.Searchers
{
    public class SearchBase
    {
        protected SearchState searchState = SearchState.Ready;
        /// <summary>
        /// Event fired when all results have been collected, or an error occurred
        /// </summary>
        public AppDelegate OnSearchCompleted { get; set; }
        public AppDelegate OnSearchStarted { get; set; }
        public TimeSpan SearchTime { get => stopwatch.Elapsed; }
        protected Stopwatch stopwatch = new Stopwatch();
        /// <summary>
        /// Indicates the current state of this search
        /// </summary>
        public SearchState SearchState
        {
            get => searchState; set
            {
                if (searchState == value)
                {
                    return;
                }

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