using MudBlazor;

namespace BLAZAM.Gui.UI.Dashboard.Widgets
{
    public partial class NewContactsWidget : TimeFrameWidget
    {
        public NewContactsWidget()
        {
            Title = Localization.AppLocalization.New_Contacts;
            WidgetType = DashboardWidgetType.NewContacts;
        }

        private List<IADContact> NewContacts
        {
            get => CurrentUser.State.Cache.Get<List<IADContact>>(this.GetType());
            set => CurrentUser.State.Cache.Set(this.GetType(), value);
        }

        protected override async Task RefreshDataAsync()
        {
            LoadingData = true;
            LoadSettings();

            NewContacts = (await Directory.Contacts.FindNewContactsAsync((int)_timeFrame?.TotalDays, false)).Where(u => u.CanRead).OrderByDescending(u => u.Created).ToList();

            LoadingData = false;

        }
        private void GoTo(DataGridRowClickEventArgs<IADContact> args)
        {
            Nav.NavigateTo(args.Item.SearchUri);
        }
    }
}
