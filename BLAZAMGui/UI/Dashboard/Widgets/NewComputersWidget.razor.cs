using MudBlazor;

namespace BLAZAM.Gui.UI.Dashboard.Widgets
{
    public partial class NewComputersWidget : Widget
    {
        public NewComputersWidget()
        {
            Title = Localization.AppLocalization.Computers_created_in_the_last_14_days;
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
            NewComputers = (await Directory.Computers.FindNewComputersAsync()).Where(u => u.CanRead).ToList();

            LoadingData = false;

        }
        void GoTo(DataGridRowClickEventArgs<IADComputer> args)
        {
            Nav.NavigateTo(args.Item.SearchUri);
        }
    }
}
