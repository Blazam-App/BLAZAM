using BLAZAM.Common;
using BLAZAM.Common.Data;
using BLAZAM.Common.Data.Services;
using BLAZAM.Nav;
using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Components;

namespace BLAZAM.Services
{
    public class SearchService
    {
        private readonly IApplicationUserStateService _userStateService;
        private readonly AppNavigationManager _nav;
        private bool includeDisabled = false;
        private string? searchTerm;

        public bool IncludeDisabled
        {
            get => includeDisabled; set
            {
                if (includeDisabled == value) return;
                includeDisabled = value;
                DisabledOptionChanged();
            }
        }
        public string? SearchTerm
        {
            get => searchTerm;
            set => searchTerm = value;
        }

        public ActiveDirectoryObjectType SeachObjectType { get; set; } = ActiveDirectoryObjectType.All;

        public SearchService(IApplicationUserStateService userStateService, AppNavigationManager nav)
        {
            _userStateService = userStateService;
            _nav = nav;
            includeDisabled = _userStateService.CurrentUserState?.Preferences?.SearchDisabledUsers == true;
        }

        private async Task DisabledOptionChanged()
        {
            try
            {
                _userStateService.CurrentUserState.Preferences.SearchDisabledUsers = IncludeDisabled;
                await _userStateService.CurrentUserState.SaveBasicUserPreferences();
            }
            catch
            {

            }
        }


        public async Task Search() => await Search(null);
        public async Task Search(string? searchTerm = null)
        {
            if (searchTerm != null)
                SearchTerm = searchTerm;


            _nav.NavigateTo("/search/" + SearchTerm);


            //Search();

        }
    }
}
