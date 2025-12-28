using Newtonsoft.Json.Linq;

namespace BLAZAM.Gui.UI.Dashboard.Widgets
{
    public partial class ChangedPasswordsWidget : TimeFrameWidget
    {
        public ChangedPasswordsWidget()
        {
            Title = Localization.AppLocalization.Changed_Passwords;
            WidgetType = DashboardWidgetType.PasswordsChanged;
            if (_timeFrame == null)
            {
                _timeFrame = TimeSpan.FromDays(90);
            }
        }

        private List<IADUser> LockedUsers = [];


        protected override async Task RefreshDataAsync()
        {
            LoadingData = true;
            LoadSettings();
            LockedUsers = (await Directory.Users.FindChangedPasswordUsersAsync((int)_timeFrame?.TotalDays,false)).Where(u => u.CanRead).ToList();
            LoadingData = false;

        }
    }
}
