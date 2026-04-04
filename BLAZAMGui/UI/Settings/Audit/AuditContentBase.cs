using BLAZAM.Database.Context;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace BLAZAM.Gui.UI.Settings.Audit
{
    public abstract class AuditContentBase<T> : DatabaseComponentBase
    {
        protected DateTime? StartDate { get; set; } = DateTime.Today.AddDays(-7);
        protected DateTime? EndDate { get; set; } = DateTime.Today;
        protected List<T> auditEntries = new();

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await FetchData();
        }

        protected virtual async Task TimeframeChanged((DateTime? start, DateTime? end) dates)
        {
            StartDate = dates.start;
            EndDate = dates.end;
            await FetchData();
        }

        protected abstract Task FetchData();
    }
}
