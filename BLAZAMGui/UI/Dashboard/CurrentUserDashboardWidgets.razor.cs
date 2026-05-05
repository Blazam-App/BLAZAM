using BLAZAM.Gui.Services;
using BLAZAM.Gui.UI.Dashboard.Widgets;
using MudBlazor;

namespace BLAZAM.Gui.UI.Dashboard
{
    public partial class CurrentUserDashboardWidgets : AppComponentBase
    {
        public bool EditMode { get; protected set; }

        private bool _initialized;

        [Inject]
        public WidgetService? WidgetService { get; set; }

        public AppDelegate<Widget> OnRefreshWidget { get; set; } = (val) => { };

        private MudDropContainer<UserDashboardWidget>? widgetContainer;

        private List<Widget> allWidgets = [];

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await StateHasChangedAsync();
            if (WidgetService == null)
            {
                Loggers.SystemLogger.Error(new AppException("WidgetService is not injected properly."), "Widget service not available");
            }
            else
            {
                allWidgets = [.. WidgetService.Available()];
            }
            await StateHasChangedAsync();

        }
        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);
            if (firstRender)
            {
                _initialized = true;
                StateHasChanged();
            }
        }
        private bool ItemSelector(UserDashboardWidget item, string dropzone)
        {
            return item.Slot == dropzone;
        }

        private Task ItemDropped(MudItemDropInfo<UserDashboardWidget> dropItem)
        {
            if (dropItem.Item == null)
            {
                return Task.CompletedTask;
            }

            if (CurrentUser.State == null)
            {
                return Task.CompletedTask;
            }

            var droppedWidget = CurrentUser.State.Preferences?.DashboardWidgets
                .FirstOrDefault(w => w.WidgetType == dropItem.Item.WidgetType);

            if (droppedWidget == null)
            {
                return Task.CompletedTask;
            }

            // Update the Slot of the dropped widget
            droppedWidget.Slot = dropItem.DropzoneIdentifier;

            // Reorder widgets in both slots
            ReorderWidgetsInSlot(CurrentUser.State.Preferences?.DashboardWidgets, droppedWidget, dropItem);

            CurrentUser.State.SaveDashboardWidgets();
            return Task.CompletedTask;
        }

        private void ReorderWidgetsInSlot(
            IList<UserDashboardWidget>? dashboardWidgets,
            UserDashboardWidget droppedWidget,
            MudItemDropInfo<UserDashboardWidget> dropItem)
        {
            if (dashboardWidgets == null)
            {
                return;
            }

            // Remove from original slot
            var originalSlotWidgets = dashboardWidgets
                .Where(w => w.Slot == droppedWidget.Slot && w.WidgetType != droppedWidget.WidgetType)
                .OrderBy(w => w.Order)
                .ToList();

            for (int i = 0; i < originalSlotWidgets.Count; i++)
            {
                originalSlotWidgets[i].Order = i;
            }

            // Remove from new slot and insert at new index
            var newSlotWidgets = dashboardWidgets
                .Where(w => w.Slot == dropItem.DropzoneIdentifier && w.WidgetType != droppedWidget.WidgetType)
                .OrderBy(w => w.Order)
                .ToList();

            newSlotWidgets.Insert(dropItem.IndexInZone, droppedWidget);

            for (int i = 0; i < newSlotWidgets.Count; i++)
            {
                newSlotWidgets[i].Order = i;
            }

            // Update the dashboardWidgets list
            int idx = 0;
            foreach (var widget in dashboardWidgets.Where(w => w.Slot == dropItem.DropzoneIdentifier).OrderBy(w => w.Order))
            {
                widget.Order = idx++;
            }
        }

        private async Task AddWidget(DashboardWidgetType widgetType)
        {
            var order = 0;
            if (CurrentUser.State != null)
            {
                if (CurrentUser.State.Preferences?.DashboardWidgets.Count > 0)
                {
                    order = CurrentUser.State.Preferences.DashboardWidgets.Max(w => w.Order) + 1;
                }
                CurrentUser.State.Preferences?.DashboardWidgets.Add(new UserDashboardWidget
                {
                    Id = 0,
                    User = CurrentUser.State.Preferences,
                    Slot = "slot1",
                    WidgetType = widgetType,
                    Order = order
                });
                await CurrentUser.State.SaveDashboardWidgets();
                await StateHasChangedAsync();
                Analytics.DashboardWidgetAdded(widgetType.ToString());
                widgetContainer?.Refresh();
            }

        }
        private async Task RemoveWidget(UserDashboardWidget widget)
        {
            if (CurrentUser.State != null)
            {
                CurrentUser.State.Preferences?.DashboardWidgets.Remove(widget);
                await CurrentUser.State.SaveDashboardWidgets();
                await StateHasChangedAsync();
                Analytics.DashboardWidgetRemoved(widget.WidgetType.ToString());
                widgetContainer?.Refresh();
            }

        }
    }
}