using BLAZAM.ActiveDirectory.Search;
using BLAZAM.Localization;
using MudBlazor;

namespace BLAZAM.Gui.UI.Dashboard.Widgets
{
    public partial class StaleComputersWidget : Widget
    {
        public StaleComputersWidget()
        {
            Title = AppLocalization.Stale_computers;
            WidgetType = DashboardWidgetType.StaleComputers;
        }

        List<IADComputer> StaleComputers
        {
            get => CurrentUser.State.Cache.Get<List<IADComputer>>("StaleComputers");
            set => CurrentUser.State.Cache.Set("StaleComputers", value);
        }


        protected override async Task RefreshDataAsync()
        {
            LoadingData = true;
            ADSearch searcher = new ADSearch(Directory);
            // searcher.FieldValues = new()
            // {
            //     new()
            //     {
            //         Field=ActiveDirectoryFields.LastLogonTimestamp,
            //         Operator=ActiveDirectoryFieldOperator.HistoricalTimeFrame,
            //         Negate=true,
            //         Value=  (DateTime.UtcNow - TimeSpan.FromDays(180))
            //     }
            // };
            searcher.Fields.LastLogonTime = (DateTime.UtcNow - TimeSpan.FromDays(180)).ToFileTimeUtc();
            searcher.ObjectTypeFilter = ActiveDirectoryObjectType.Computer;
            searcher.EnabledOnly = true;
            searcher.MaxResults = 200;
            var results = await searcher.SearchAsync();
            StaleComputers = results.Where(u => u.CanRead).Cast<IADComputer>().OrderByDescending(u => u.LastLogonTimestamp).ToList();
            LoadingData = false;

        }
        void GoTo(DataGridRowClickEventArgs<IADComputer> args)
        {
            Nav.NavigateTo(args.Item.SearchUri);
        }
    }
}
