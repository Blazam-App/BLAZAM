namespace BLAZAM.Gui.UI.Dashboard.Widgets
{
    public partial class ChangedPasswordsWidget : Widget
    {
        public ChangedPasswordsWidget()
        {
            Title = Localization.AppLocalization.Passwords_changed_in_the_last_90_days;
            WidgetType = DashboardWidgetType.PasswordsChanged;
        }

        private List<IADUser> LockedUsers = [];


        protected override async Task RefreshDataAsync()
        {
            LoadingData = true;
            LockedUsers = (await Directory.Users.FindChangedPasswordUsersAsync(false)).Where(u => u.CanRead).ToList();
            LoadingData = false;

        }
    }
}
