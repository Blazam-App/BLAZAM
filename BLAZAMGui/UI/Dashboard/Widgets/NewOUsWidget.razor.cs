using MudBlazor;

namespace BLAZAM.Gui.UI.Dashboard.Widgets
{
    public partial class NewOUsWidget : TimeFrameWidget
    {
        public NewOUsWidget()
        {
            Title = Localization.AppLocalization.New_OUs;
            WidgetType = DashboardWidgetType.NewOus;
        }

        private List<IADOrganizationalUnit> NewOUs
        {
            get => CurrentUser.State.Cache.Get<List<IADOrganizationalUnit>>(this.GetType());
            set => CurrentUser.State.Cache.Set(this.GetType(), value);
        }


        protected override async Task RefreshDataAsync()
        {
            LoadingData = true;
            NewOUs = (await Directory.OUs.FindNewOUsAsync((int)_timeFrame?.TotalDays)).Where(u => u.CanRead).ToList();

            LoadingData = false;

        }
        private void GoTo(DataGridRowClickEventArgs<IADOrganizationalUnit> args)
        {
            Nav.NavigateTo(args.Item.SearchUri);
        }
    }
}
