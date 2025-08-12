using BLAZAM.Localization;
using MudBlazor;

namespace BLAZAM.Gui.UI.Dashboard.Widgets
{
    public partial class NewGroupsWidget : Widget
    {
        public NewGroupsWidget()
        {
            Title = string.Format(Localization.AppLocalization.Groups_created_in_the_last_14_days);
            WidgetType = DashboardWidgetType.NewGroups;
        }

        List<IADGroup> NewGroups
        {
            get => CurrentUser.State.Cache.Get<List<IADGroup>>(this.GetType());
            set => CurrentUser.State.Cache.Set(this.GetType(), value);
        }

        protected override async Task RefreshDataAsync()
        {
            LoadingData = true;
            NewGroups = (await Directory.Groups.FindNewGroupsAsync()).Where(u => u.CanRead).ToList();

            LoadingData = false;

        }
        void GoTo(DataGridRowClickEventArgs<IADGroup> args)
        {
            Nav.NavigateTo(args.Item.SearchUri);
        }
    }
}
