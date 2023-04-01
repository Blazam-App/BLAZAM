using Microsoft.AspNetCore.Components;

namespace BLAZAM.Server.Data.Services
{
    public class AppNavigationManager
    {
        NavigationManager NavigationManager { get; set; }

        public AppNavigationManager(NavigationManager navigationManager)
        {
            NavigationManager = navigationManager;
        }
        /// <summary>
        /// Navigates to the specified URI.
        /// </summary>
        /// <param name="uri">The destination URI. This can be absolute, or relative to the base URI
        /// (as returned by <see cref="BaseUri"/>).</param>
        /// <param name="forceLoad">If true, bypasses client-side routing and forces the browser to load the new page from the server, whether or not the URI would normally be handled by the client-side router.</param>
        /// <param name="replace">If true, replaces the current entry in the history stack. If false, appends the new entry to the history stack.</param>
        public void NavigateTo(string uri, bool forceLoad = false, bool replace = false)
        {
            NavigationManager.NavigateTo(uri, forceLoad, replace);
        }

        /// <summary>
        /// Gets or sets the current base URI. The <see cref="BaseUri" /> is always represented as an absolute URI in string form with trailing slash.
        /// Typically this corresponds to the 'href' attribute on the document's &lt;base&gt; element.
        /// </summary>
        /// <remarks>
        /// Setting <see cref="BaseUri" /> will not trigger the <see cref="LocationChanged" /> event.
        /// </remarks>
        public string BaseUri=>NavigationManager.BaseUri;


        /// <summary>
        /// Gets or sets the current URI. The <see cref="Uri" /> is always represented as an absolute URI in string form.
        /// </summary>
        /// <remarks>
        /// Setting <see cref="Uri" /> will not trigger the <see cref="LocationChanged" /> event.
        /// </remarks>
        public string Uri => NavigationManager.BaseUri;


    }
}
