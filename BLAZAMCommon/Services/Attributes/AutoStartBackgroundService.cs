namespace BLAZAM.Common.Attributes
{
    public class AutoStartBackgroundService : Attribute
    {
        public int IntervalInMinutes { get; set; }
        public bool Immediate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interval">The time in minutes this service should execute</param>
        public AutoStartBackgroundService(int interval, bool immediate = false)
        {
            IntervalInMinutes = interval;
            Immediate = immediate;
        }
    }
}
