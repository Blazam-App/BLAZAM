using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Services.Attributes
{
    public class AutoStartBackgroundService:System.Attribute
    {
        public int IntervalInMinutes { get; set; }
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interval">The time in minutes this service should execute</param>
        public AutoStartBackgroundService(int interval)
        {
            IntervalInMinutes = interval;
        }
    }
}
