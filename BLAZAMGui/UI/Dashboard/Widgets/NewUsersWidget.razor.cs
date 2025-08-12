using MudBlazor;

namespace BLAZAM.Gui.UI.Dashboard.Widgets
{
    public partial class NewUsersWidget : Widget
    {
        public NewUsersWidget()
        {
            Title = AppLocalization["Users created in the last 14 days"];
            WidgetType = DashboardWidgetType.NewUsers;
        }

        List<IADUser> NewUsers
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
        void GoTo(DataGridRowClickEventArgs<IADUser> args)
        {
            Nav.NavigateTo(args.Item.SearchUri);
        }
    }
}
