using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace BLAZAM.Gui.UI.Shared
{
    public partial class TimeframeSelector : AppComponentBase
    {
        [Parameter]
        public DateTime? StartDate { get; set; } = DateTime.Today.AddDays(-7);

        [Parameter]
        public DateTime? EndDate { get; set; } = DateTime.Today;

        [Parameter]
        public EventCallback<(DateTime?, DateTime?)> OnTimeframeChanged { get; set; }

        private async Task Apply()
        {
            await OnTimeframeChanged.InvokeAsync((StartDate, EndDate));
        }
    }
}
