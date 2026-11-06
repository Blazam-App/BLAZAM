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

        private List<IADUser> LockedUsers
        {
            get => CurrentUser.State?.Cache.Get<List<IADUser>>(this.GetType());
            set => CurrentUser.State?.Cache.Set(this.GetType(), value);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

        }

        protected override void RefreshDataAsync()
        {
            _ = Task.Run(() =>
            {
                LoadingData = true;
                LockedUsers = Directory.Users.FindLockedOutUsers().OrderByDescending(u => u.LockoutTime).Where(u => u.CanRead).ToList();
                LoadingData = false;
            });
        }

        private void GoTo(DataGridRowClickEventArgs<IADUser> args)
        {
            Nav.NavigateTo(args.Item.SearchUri);
        }

    }
}