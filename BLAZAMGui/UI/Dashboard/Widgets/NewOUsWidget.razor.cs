using BLAZAM.Localization;
using MudBlazor;

namespace BLAZAM.Gui.UI.Dashboard.Widgets
{
    public partial class NewOUsWidget : Widget
    {
        public NewOUsWidget()
        {
            Title = string.Format(Localization.AppLocalization.OUs_created_in_the_last_14_days);
            WidgetType = DashboardWidgetType.NewOus;
        }

        List<IADOrganizationalUnit> NewOUs
        {
            get => CurrentUser.State.Cache.Get<List<IADOrganizationalUnit>>(this.GetType());
            set => CurrentUser.State.Cache.Set(this.GetType(), value);
        }


        protected override async Task RefreshDataAsync()
        {
            LoadingData = true;
            NewOUs = (await Directory.OUs.FindNewOUsAsync()).Where(u => u.CanRead).ToList();

            LoadingData = false;

        }
        void GoTo(DataGridRowClickEventArgs<IADOrganizationalUnit> args)
        {
            Nav.NavigateTo(args.Item.SearchUri);
        }
    }
}
