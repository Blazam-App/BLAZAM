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

        protected async Task SetTimeFrame(TimeSpan? timeFrame)
        {
            _timeFrame = timeFrame;
            await SetWidgetJson(timeFrame);
            
        }


    }
}
