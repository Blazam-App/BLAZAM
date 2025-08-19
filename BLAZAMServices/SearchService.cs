using BLAZAM.Logger;
using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Components;

namespace BLAZAM.Services
{
    /// <summary>
    /// Service responsible for managing search functionality, including search terms and user preferences related to search.
    /// </summary>
    public class SearchService
    {
        private readonly IApplicationUserStateService _userStateService;
        private readonly NavigationManager _nav;
        private bool includeDisabled = false;
        private string? searchTerm;

        /// <summary>
        /// Gets or sets a value indicating whether disabled AD objects should be included in search results. Persists this preference.
        /// </summary>
        public bool IncludeDisabled
        {
            get => includeDisabled; set
            {
                if (includeDisabled == value) return;
                includeDisabled = value;
                DisabledOptionChanged();
            }
        }

        /// <summary>
        /// Gets or sets the current search term string.
        /// </summary>
        public string? SearchTerm
        {
            get => searchTerm;
            set => searchTerm = value;
        }

        /// <summary>
        /// Gets or sets the type of Active Directory object to filter searches by. Defaults to All.
        /// </summary>
        public ActiveDirectoryObjectType SeachObjectType { get; set; } = ActiveDirectoryObjectType.All;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchService"/> class.
        /// </summary>
        /// <param name="userStateService">Service for accessing current user state and preferences.</param>
        /// <param name="nav">Navigation manager for redirecting to search pages.</param>
        /// <exception cref="ArgumentNullException">Thrown if userStateService or nav is null.</exception>
        public SearchService(IApplicationUserStateService userStateService, NavigationManager nav)
        {
            ArgumentNullException.ThrowIfNull(userStateService);

            ArgumentNullException.ThrowIfNull(nav);



            _userStateService = userStateService;
            _nav = nav;
            includeDisabled = _userStateService.CurrentUserState?.Preferences?.SearchDisabledUsers == true;
        }

        /// <summary>
        /// Handles the change in the IncludeDisabled option and saves the user's preference.
        /// </summary>
        private async Task DisabledOptionChanged()
        {
            if (_userStateService.CurrentUserState == null)
            {
                Loggers.SystemLogger.Warning("SearchService.DisabledOptionChanged: _userStateService.CurrentUserState is null. Cannot save preferences.");
                return;
            }
            try
            {
                _userStateService.CurrentUserState.Preferences.SearchDisabledUsers = IncludeDisabled;
                await _userStateService.CurrentUserState.SaveBasicUserPreferences();
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Warning(ex, "SearchService.DisabledOptionChanged: Exception caught while trying to save user preferences for SearchDisabledUsers.");
            }
        }

        /// <summary>
        /// Asynchronously performs a search using the current <see cref="SearchTerm"/> by navigating to the search page.
        /// </summary>
        public async Task SearchAsync() => await Task.Run(() => { Search(null); });

        /// <summary>
        /// Performs a search. If a searchTerm parameter is provided, it updates the current <see cref="SearchTerm"/>. Navigates to the search page.
        /// </summary>
        /// <param name="searchTerm">Optional search term to set before navigating.</param>
        public void Search(string? searchTerm = null)
        {
            Loggers.SystemLogger.Debug("SearchService.Search: Search called. Current SearchTerm: '{CurrentSearchTerm}', Provided searchTerm parameter: '{ProvidedSearchTerm}'", SearchTerm, searchTerm);
            if (searchTerm != null)
                SearchTerm = searchTerm;

            Loggers.SystemLogger.Debug("SearchService.Search: Navigating to /search/{FinalSearchTerm}", SearchTerm);
            _nav.NavigateTo("/search/" + SearchTerm);



        }
    }
}
