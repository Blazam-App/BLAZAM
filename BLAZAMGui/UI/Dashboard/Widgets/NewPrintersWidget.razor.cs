using BLAZAM.Localization;
using MudBlazor;

namespace BLAZAM.Gui.UI.Dashboard.Widgets
{
    public partial class NewPrintersWidget : Widget
    {
        public NewPrintersWidget()
        {
            Title = string.Format(Localization.AppLocalization.Printers_created_in_the_last_14_days);
            WidgetType = DashboardWidgetType.NewPrinters;
        }

        List<IADPrinter> NewPrinters
        {
            get => CurrentUser.State.Cache.Get<List<IADPrinter>>(this.GetType());
            set => CurrentUser.State.Cache.Set(this.GetType(), value);
        }


        protected override async Task RefreshDataAsync()
        {
            LoadingData = true;
            NewPrinters = (await Directory.Printers.FindNewPrintersAsync(14, false)).Where(u => u.CanRead).ToList();

            LoadingData = false;

        }
        void GoTo(DataGridRowClickEventArgs<IADPrinter> args)
        {
            Nav.NavigateTo(args.Item.SearchUri);
        }
    }
}
