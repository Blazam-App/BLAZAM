using BLAZAM.Common.Data.ActiveDirectory;
using BLAZAM.Common.Data.Services;
using Microsoft.AspNetCore.Components;

namespace BLAZAM.Server.Data.Services
{
    public class SearchService
    {
        private readonly IApplicationUserStateService _userStateService;
        private readonly NavigationManager _nav;
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

        public SearchService(IApplicationUserStateService userStateService, NavigationManager nav)
        {
            _userStateService = userStateService;
            _nav = nav;
            includeDisabled = _userStateService.CurrentUserState?.UserSettings?.SearchDisabledUsers == true;
        }





        async Task DisabledOptionChanged()
        {
            try
            {
                _userStateService.CurrentUserState.UserSettings.SearchDisabledUsers = IncludeDisabled;
                await _userStateService.CurrentUserState.SaveUserSettings();
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
