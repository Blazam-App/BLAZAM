using MudBlazor;

namespace BLAZAM.Gui.UI.Dashboard.Widgets
{
    public partial class NewComputersWidget : TimeFrameWidget
    {
        public NewComputersWidget()
        {
            Title = Localization.AppLocalization.New_Computers;
            WidgetType = DashboardWidgetType.NewComputers;
        }

        private List<IADComputer> NewComputers
        {
            get => CurrentUser.State.Cache.Get<List<IADComputer>>(this.GetType());
            set => CurrentUser.State.Cache.Set(this.GetType(), value);
        }

        protected override async Task RefreshDataAsync()
        {
            LoadingData = true;

            NewComputers = (await Directory.Computers.FindNewComputersAsync((int)_timeFrame!.Value.TotalDays)).Where(u => u.CanRead).ToList();

            LoadingData = false;

        }
        private void GoTo(DataGridRowClickEventArgs<IADComputer> args)
        {
            Nav.NavigateTo(args.Item.SearchUri);
        }
    }
}
