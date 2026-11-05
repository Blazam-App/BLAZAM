namespace BLAZAM.Gui.UI.Settings.Audit
{
    public abstract class AuditContentBase<T> : DatabaseComponentBase
    {
        protected DateTime? StartDate { get; set; } = DateTime.Today.ToLocalTime().AddDays(-7);
        protected DateTime? EndDate { get; set; } = DateTime.Today.ToLocalTime();
        protected List<T> auditEntries = new();

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await FetchData();
        }

        protected virtual async Task TimeframeChanged((DateTime? start, DateTime? end) dates)
        {
            StartDate = dates.start?.ToLocalTime();
            EndDate = dates.end?.ToLocalTime();
            await FetchData();
        }

        protected abstract Task FetchData();
    }
}
