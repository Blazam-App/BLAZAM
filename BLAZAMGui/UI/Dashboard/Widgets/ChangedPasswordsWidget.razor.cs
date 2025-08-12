namespace BLAZAM.Gui.UI.Dashboard.Widgets
{
    public partial class ChangedPasswordsWidget : Widget
    {
        public ChangedPasswordsWidget()
        {
            Title = AppLocalization["Passwords changed in the last 90 days"];
            WidgetType = DashboardWidgetType.PasswordsChanged;
        }

        List<IADUser> LockedUsers = new();


        protected override async Task RefreshDataAsync()
        {
            LoadingData = true;
            LockedUsers = (await Directory.Users.FindChangedPasswordUsersAsync(false)).Where(u => u.CanRead).ToList();
            LoadingData = false;

        }
    }
}
