using BLAZAM.Database.Models.Audit;

namespace BLAZAM.Gui.UI.Dashboard.Widgets.Admin
{
    public partial class AppLogonsWidget : Widget
    {
        public AppLogonsWidget()
        {
            Title = string.Format(Localization.AppLocalization.Application_logons);
            WidgetType = DashboardWidgetType.AppLogons;
        }

        private List<LogonAuditLog> UserLogons
        {
            get => CurrentUser.State?.Cache.Get<List<LogonAuditLog>>(this.GetType());
            set => CurrentUser.State?.Cache.Set(this.GetType(), value);
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }

        protected override async Task RefreshDataAsync()
        {
            LoadingData = true;
            try
            {
                UserLogons = (await Context.LogonAuditLog.OrderByDescending(u => u.Timestamp).Take(50).ToListAsync());
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex, "Failed to load user logons for AppLogonsWidget");
            }
            LoadingData = false;
        }
    }
}
