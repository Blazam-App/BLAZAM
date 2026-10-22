namespace BLAZAM.Gui.UI.Dashboard.Widgets
{
    public abstract class TimeFrameWidget : Widget
    {
        protected virtual TimeSpan? _timeFrame => JsonSettings?.FromJson<TimeSpan>() ?? TimeSpan.FromDays(14);

        protected async Task SetTimeFrame(TimeSpan? timeFrame)
        {
            JsonSettings = timeFrame.ToJson();
            await SetWidgetJson(timeFrame);

        }


    }
}
