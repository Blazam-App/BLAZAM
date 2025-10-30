using MudBlazor;

namespace BLAZAM.Gui.UI
{
    public partial class DirectoryEntryViewHeader : DirectoryEntryViewBase
    {
        private AppModal? _requestAccessModal;
        private AppModal? _appEventModal;
        private bool _showRequestButton;

        [Parameter]
        public EventCallback OnDelete { get; set; }

        [Parameter]
        public EventCallback OnRebootOrShutdown { get; set; }

        [Parameter]
        public EventCallback OnMove { get; set; }


        [Parameter]
        public EventCallback OnRename { get; set; }



        [Parameter]
        public EventCallback OnResetPassword { get; set; }


        [Parameter]
        public EventCallback OnAssignTo { get; set; }


        [Parameter]
        public EventCallback OnUnlock { get; set; }


        [Parameter]
        public EventCallback OnEnable { get; set; }

        [Parameter]
        public EventCallback OnDisable { get; set; }


        [Parameter]
        public EventCallback OnShowHistory { get; set; }

        [Parameter]
        public EventCallback OnShowPermissions { get; set; }



        /// <summary>
        /// Called when the edit mode is changed
        /// </summary>
        [Parameter]
        public EventCallback<bool> OnToggleEditMode { get; set; }

        protected override async Task OnInitializedAsync()
        {

            await base.OnInitializedAsync();
            LoadingData = true;

            _showRequestButton = (await Context.GlobalPermissionSettings.FirstOrDefaultAsync())?.AllowAccessRequest == true
            && await Context.GlobalPermissionRequestActions.CountAsync() > 0;
            LoadingData = false;

        }

        private async Task ToggleEditMode(bool editEnabled)
        {
            EditMode = editEnabled;
            await OnToggleEditMode.InvokeAsync(editEnabled);
        }
        private bool IsFavorite
        {
            get
            {
                if (DirectoryEntry.DN == null)
                {
                    return false;
                }

                UserFavoriteEntry newFavorrite = new UserFavoriteEntry { DN = DirectoryEntry.DN, UserId = CurrentUser.State.Id };

                return CurrentUser.State.Preferences.FavoriteEntries.Any(f => f.Equals(newFavorrite));
            }
        }
        private async Task ToggleFavorite()
        {
            try
            {
                if (DirectoryEntry.DN == null)
                {
                    return;
                }

                UserFavoriteEntry? newFavorrite = new UserFavoriteEntry { DN = DirectoryEntry.DN, UserId = CurrentUser.State.Id };
                if (CurrentUser.State.Preferences.FavoriteEntries.Any(f => f.Equals(newFavorrite)))
                {
                    var matchingFavorites = CurrentUser.State.Preferences.FavoriteEntries.Where(u => newFavorrite.Equals(u)).ToList();
                    foreach (var favorite in matchingFavorites)
                    {
                        CurrentUser.State.Preferences.FavoriteEntries.Remove(favorite);
                    }
                }
                else
                {
                    CurrentUser.State.Preferences.FavoriteEntries.Add(newFavorrite);
                }
                await CurrentUser.State.SaveBasicUserPreferences();
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex, "Error toggling favorite entry");
                SnackBarService.Error("Error while trying to toggle favorite");

            }
        }

    }
}