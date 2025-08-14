using MudBlazor;

namespace BLAZAM.Gui.UI.Dashboard.Widgets
{
    public partial class LockedOutUsers : Widget
    {
        public LockedOutUsers()
        {
            Title = Localization.AppLocalization.Locked_Out_Users;
            WidgetType = DashboardWidgetType.LockedOutUsers;
        }

        List<IADUser> LockedUsers
        {
            get => CurrentUser.State?.Cache.Get<List<IADUser>>(this.GetType());
            set => CurrentUser.State?.Cache.Set(this.GetType(), value);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

        }

        protected override async Task RefreshDataAsync()
        {
            LoadingData = true;
            LockedUsers = (await Directory.Users.FindLockedOutUsersAsync()).OrderByDescending(u => u.LockoutTime).Where(u => u.CanRead).ToList();
            LoadingData = false;
        }

        void GoTo(DataGridRowClickEventArgs<IADUser> args)
        {
            Nav.NavigateTo(args.Item.SearchUri);
        }

    }
}