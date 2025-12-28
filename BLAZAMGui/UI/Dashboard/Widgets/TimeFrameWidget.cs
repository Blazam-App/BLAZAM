using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Gui.UI.Dashboard.Widgets
{
    public abstract class TimeFrameWidget:Widget
    {
        protected TimeSpan? _timeFrame = TimeSpan.FromDays(14);

        public TimeFrameWidget():base()
        {
            LoadSettings();
        }
        protected async Task SetTimeFrame(TimeSpan? timeFrame)
        {
            _timeFrame = timeFrame;
            await SetWidgetJson(timeFrame);
            
        }
        protected void LoadSettings()
        {
            TimeSpan? jsonTimespan = JsonSettings?.FromJson<TimeSpan>();
            if (jsonTimespan.HasValue)
            {
                _timeFrame = jsonTimespan;
            }
        }

    }
}
