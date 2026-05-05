using MudBlazor;

namespace BLAZAM.Gui.UI.Dashboard.Widgets
{
    public partial class NewUsersWidget : TimeFrameWidget
    {
        public NewUsersWidget()
        {
            Title = Localization.AppLocalization.New_Users;
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
            NewUsers = (await Directory.Users.FindNewUsersAsync((int)_timeFrame!.Value.TotalDays, false)).Where(u => u.CanRead).OrderByDescending(u => u.Created).ToList();

            LoadingData = false;

        }
        private void GoTo(DataGridRowClickEventArgs<IADUser> args)
        {
            Nav.NavigateTo(args.Item.SearchUri);
        }
    }
}
