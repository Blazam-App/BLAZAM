namespace BLAZAM.Gui.UI.Dashboard.Widgets
{
    public partial class DisabledUsersWidget : Widget
    {
        public DisabledUsersWidget()
        {
            Title = Localization.AppLocalization.Disabled_users_changed_in_the_last_90_days;
            WidgetType = DashboardWidgetType.DisabledUsers;
        }

        List<IADUser> DisabledUsers
        {
            get => CurrentUser.State.Cache.Get<List<IADUser>>(this.GetType());
            set => CurrentUser.State.Cache.Set(this.GetType(), value);
        }



        protected override async Task RefreshDataAsync()
        {
            LoadingData = true;
            DisabledUsers = (await Directory.Users.FindChangedUsersAsync(false)).Where(u => u.Disabled).ToList();

            LoadingData = false;

        }
    }
}
