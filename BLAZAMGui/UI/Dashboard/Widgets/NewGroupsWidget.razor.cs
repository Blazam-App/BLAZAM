using MudBlazor;

namespace BLAZAM.Gui.UI.Dashboard.Widgets
{
    public partial class NewGroupsWidget : TimeFrameWidget
    {
        public NewGroupsWidget()
        {
            Title = Localization.AppLocalization.New_Groups;
            WidgetType = DashboardWidgetType.NewGroups;
        }

        private List<IADGroup> NewGroups
        {
            get => CurrentUser.State.Cache.Get<List<IADGroup>>(this.GetType());
            set => CurrentUser.State.Cache.Set(this.GetType(), value);
        }

        protected override async Task RefreshDataAsync()
        {
            LoadingData = true;

            NewGroups = (await Directory.Groups.FindNewGroupsAsync((int)_timeFrame!.Value.TotalDays)).Where(u => u.CanRead).ToList();

            LoadingData = false;

        }
        private void GoTo(DataGridRowClickEventArgs<IADGroup> args)
        {
            Nav.NavigateTo(args.Item.SearchUri);
        }
    }
}
