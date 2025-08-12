using BLAZAM.Localization;

namespace BLAZAM.Gui.UI.Dashboard.Widgets
{
    public partial class ChangedPasswordsWidget : Widget
    {
        public ChangedPasswordsWidget()
        {
            Title = string.Format(Localization.AppLocalization.Passwords_changed_in_the_last_90_days);
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
