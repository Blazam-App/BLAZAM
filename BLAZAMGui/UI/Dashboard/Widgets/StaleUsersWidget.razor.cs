using MudBlazor;

namespace BLAZAM.Gui.UI.Dashboard.Widgets
{
    public partial class StaleUsersWidget : Widget
    {
        public StaleUsersWidget()
        {
            Title = Localization.AppLocalization.Stale_users;
            WidgetType = DashboardWidgetType.StaleUsers;
        }

        private List<IADUser> StaleUsers
        {
            get => CurrentUser.State.Cache.Get<List<IADUser>>("StaleUsers");
            set => CurrentUser.State.Cache.Set("StaleUsers", value);
        }


        protected override async Task RefreshDataAsync()
        {

            LoadingData = true;
            ADSearch searcher = new ADSearch(Directory);
            searcher.Fields.LastLogonTime = (DateTime.UtcNow - TimeSpan.FromDays(180)).ToFileTimeUtc();
            searcher.ObjectTypeFilter = ActiveDirectoryObjectType.User;
            searcher.EnabledOnly = true;
            searcher.MaxResults = 200;
            var results = await searcher.SearchAsync();
            StaleUsers = results.Where(u => u.CanRead).Cast<IADUser>().OrderByDescending(u => u.LastLogonTimestamp).ToList();
            LoadingData = false;


        }
        void GoTo(DataGridRowClickEventArgs<IADUser> args)
        {
            Nav.NavigateTo(args.Item.SearchUri);
        }
    }
}
