using MudBlazor;

namespace BLAZAM.Gui.UI.Dashboard.Widgets
{
    public partial class NewUsersWidget : Widget
    {
        public NewUsersWidget()
        {
            Title = Localization.AppLocalization.Users_created_in_the_last_14_days;
            WidgetType = DashboardWidgetType.NewUsers;
        }

        private List<IADUser> NewUsers
        {
            get => CurrentUser.State.Cache.Get<List<IADUser>>(this.GetType());
            set => CurrentUser.State.Cache.Set(this.GetType(), value);
        }

        protected override async Task RefreshDataAsync()
        {
            LoadingData = true;
            NewUsers = (await Directory.Users.FindNewUsersAsync(14, false)).Where(u => u.CanRead).OrderByDescending(u => u.Created).ToList();

            LoadingData = false;

        }
        private void GoTo(DataGridRowClickEventArgs<IADUser> args)
        {
            Nav.NavigateTo(args.Item.SearchUri);
        }
    }
}
