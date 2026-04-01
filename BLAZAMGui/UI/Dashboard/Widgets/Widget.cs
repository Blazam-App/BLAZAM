using MudBlazor;

namespace BLAZAM.Gui.UI.Dashboard.Widgets
{
    public abstract class Widget : DatabaseComponentBase
    {

        [Parameter]
        public RenderFragment? WidgetContent { get; set; }
        [Parameter]
        public TimeSpan TimeSpan { get; set; }
        public string Title { get; set; }
        [Parameter]
        public DashboardWidgetType WidgetType { get; set; }
        [Parameter]
        public int ItemsPerPage { get; set; } = 5;
        [Parameter]
        public string? JsonSettings { get; set; }   
        [CascadingParameter]
        public CurrentUserDashboardWidgets CurrentUserDashboardWidgets { get; set; }


        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            if (CurrentUserDashboardWidgets?.OnRefreshWidget != null)
            {
                CurrentUserDashboardWidgets.OnRefreshWidget += (Widget widget) =>
                {
                    if (widget.WidgetType.Equals(WidgetType))
                    {
                        _ = RefreshDataAsync();

                    }

                };
            }
            _ = Task.Run(() =>
            {
                _ = RefreshDataAsync();

            });
        }



        protected abstract Task RefreshDataAsync();

        protected virtual void RowClicked(DataGridRowClickEventArgs<IDirectoryEntryAdapter> args)
        {
            if (args.Item != null)
            {
                GoTo(args.Item);
            }
        }
        protected virtual void GoTo(IDirectoryEntryAdapter args)
        {
            Nav.NavigateTo(args.SearchUri);
        }

        protected async Task SetRowsPerPage(int rowPerPage)
        {
            try
            {
                CurrentUser.State.Preferences.DashboardWidgets.First(w => w.WidgetType.Equals(WidgetType)).ItemsPerPage = rowPerPage;
                await CurrentUser.State.SaveDashboardWidgets();
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex,"Error attempting to set a widgets rows per page preference.");
            }
        }

        protected async Task SetWidgetJson(object? obj)
        {
            try
            {
                CurrentUser.State.Preferences.DashboardWidgets.First(w => w.WidgetType.Equals(WidgetType)).JsonSettings = obj.ToJson();
                await CurrentUser.State.SaveDashboardWidgets();
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex,"Error attempting to set a widgets rows per page preference.");
            }
        }

    }
}