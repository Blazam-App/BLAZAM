using MudBlazor;

namespace BLAZAM.Gui.UI.Dashboard.Widgets
{
    public partial class NewPrintersWidget : TimeFrameWidget
    {
        public NewPrintersWidget()
        {
            Title = Localization.AppLocalization.New_Printers;
            WidgetType = DashboardWidgetType.NewPrinters;
        }

        private List<IADPrinter> NewPrinters
        {
            get => CurrentUser.State.Cache.Get<List<IADPrinter>>(this.GetType());
            set => CurrentUser.State.Cache.Set(this.GetType(), value);
        }


        protected override async Task RefreshDataAsync()
        {
            LoadingData = true;
            NewPrinters = (await Directory.Printers.FindNewPrintersAsync((int)_timeFrame?.TotalDays, false)).Where(u => u.CanRead).ToList();

            LoadingData = false;

        }
        private void GoTo(DataGridRowClickEventArgs<IADPrinter> args)
        {
            Nav.NavigateTo(args.Item.SearchUri);
        }
    }
}
