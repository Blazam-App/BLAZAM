using BLAZAM.Database.Models.Audit;
using BLAZAM.Database.Models.User;
using BLAZAM.Localization;
using MudBlazor;

namespace BLAZAM.Gui.UI.Dashboard.Widgets.Admin
{
    public partial class AppLogonsWidget : Widget
    {
        public AppLogonsWidget()
        {
            Title = string.Format(Localization.AppLocalization.Application_logons);
            WidgetType = DashboardWidgetType.AppLogons;
        }

        List<LogonAuditLog> UserLogons
        {
            get => CurrentUser.State?.Cache.Get<List<LogonAuditLog>>(this.GetType());
            set => CurrentUser.State?.Cache.Set(this.GetType(), value);
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            CurrentUserDashboardWidgets.OnRefreshWidget += (Widget widget) =>
            {
                if (widget.WidgetType.Equals(WidgetType))
                {
                    _ = RefreshDataAsync();

                }

            };

            _ = RefreshDataAsync();
        }

        protected override async Task RefreshDataAsync()
        {
            LoadingData = true;
            await InvokeAsync(StateHasChanged);
            UserLogons = (await Context.LogonAuditLog.OrderByDescending(u => u.Timestamp).Take(50).ToListAsync());
            LoadingData = false;
            await InvokeAsync(StateHasChanged);
        }

        void GoTo(DataGridRowClickEventArgs<IADUser> args)
        {
            Nav.NavigateTo(args.Item.SearchUri);
        }
    }
}
