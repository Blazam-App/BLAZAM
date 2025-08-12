using MudBlazor;

namespace BLAZAM.Gui.UI.Dashboard.Widgets
{
    public partial class NewContactsWidget : Widget
    {
        public NewContactsWidget()
        {
            Title = AppLocalization["Contacts created in the last 14 days"];
            WidgetType = DashboardWidgetType.NewContacts;
        }

        List<IADContact> NewContacts
        {
            get => CurrentUser.State.Cache.Get<List<IADContact>>(this.GetType());
            set => CurrentUser.State.Cache.Set(this.GetType(), value);
        }

        protected override async Task RefreshDataAsync()
        {
            LoadingData = true;
            NewContacts = (await Directory.Contacts.FindNewContactsAsync(14, false)).Where(u => u.CanRead).OrderByDescending(u => u.Created).ToList();

            LoadingData = false;

        }
        void GoTo(DataGridRowClickEventArgs<IADContact> args)
        {
            Nav.NavigateTo(args.Item.SearchUri);
        }
    }
}
