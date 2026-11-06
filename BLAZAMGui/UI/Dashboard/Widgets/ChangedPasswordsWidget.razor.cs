namespace BLAZAM.Gui.UI.Dashboard.Widgets
{
    public partial class ChangedPasswordsWidget : TimeFrameWidget
    {
        protected override TimeSpan? _timeFrame => JsonSettings?.FromJson<TimeSpan>() ?? TimeSpan.FromDays(90);

        public ChangedPasswordsWidget()
        {
            Title = Localization.AppLocalization.Changed_Passwords;
            WidgetType = DashboardWidgetType.PasswordsChanged;
        }

        private List<IADUser> ChangedPasswords = [];


        protected override void RefreshDataAsync()
        {
            _ = Task.Run(() =>
            {
                LoadingData = true;
                ChangedPasswords = Directory.Users.FindChangedPasswordUsers((int)_timeFrame!.Value.TotalDays, false).Where(u => u.CanRead).ToList();
                LoadingData = false;
            });


        }
    }
}
