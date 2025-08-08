using System.Diagnostics;

namespace BLAZAM.Common.Data
{
    public static class ApplicationStatistics
    {
        public static Process Process { get; set; }
        public static int ADContextCount { get; private set; }
        public static void AddADContext()
        {
            ADContextCount++;
        }
        public static void RemoveADContext()
        {
            if (ADContextCount > 0)
                ADContextCount--;

        }


        public static int DBContextCount { get; private set; }
        public static void AddDBContext()
        {
            DBContextCount++;
        }
        public static void RemoveDBContext()
        {
            if (DBContextCount > 0)
                DBContextCount--;

        }
        public static RollingAverage MemoryUsage { get; private set; } = new(5);
        public static RollingAverage CPUUsage { get; private set; } = new(5);
        private static Timer? _resourceUsageTimer;
        public static void StartResourceUsagePolling()
        {
            if (_resourceUsageTimer == null)
            {
                _resourceUsageTimer = new Timer((state) => { PollData(); }, null, 0, 2000);
            }
        }
        public static async Task StopResourceUsagePolling()
        {
            if (_resourceUsageTimer != null)
            {
                await _resourceUsageTimer.DisposeAsync();
                _resourceUsageTimer = null;
            }
        }
        private static void PollData()
        {

            try
            {
                Process.Refresh();
                MemoryUsage.AddValue(Process.WorkingSet64);
                var processTimeDelta = Process.TotalProcessorTime - lastProcessTime;
                lastProcessTime = Process.TotalProcessorTime;
                var pollingDelta = DateTime.Now - lastPollTime;
                var cpuValue = (processTimeDelta / pollingDelta) * 100;
                if (cpuValue <= 100)
                {
                    CPUUsage.AddValue(cpuValue);
                }
                lastPollTime = DateTime.Now;

            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex, "Error creating performance counters");
            }


        }
        private static TimeSpan lastProcessTime = TimeSpan.Zero;
        private static DateTime lastPollTime = DateTime.Now;
    }
}
