using BLAZAM.Global.Data;
using BLAZAM.Global.Events;
using System.Diagnostics;

namespace BLAZAM.Common.Data
{
    /// <summary>
    /// Provides centralized tracking and monitoring of application-wide statistics including
    /// connection counts, resource usage metrics, and performance data.
    /// </summary>
    public static class ApplicationStatistics
    {
        /// <summary>
        /// Event raised when the LDAP connection count changes.
        /// </summary>
        public static AppEvent<int> OnLdapCountChanged { get; }  = new ();
        
        /// <summary>
        /// The current application process instance used for performance monitoring.
        /// </summary>
        public static Process Process { get; set; }
        
        /// <summary>
        /// Gets the current count of active Active Directory contexts.
        /// </summary>
        public static int ADContextCount { get; private set; }
        
        /// <summary>
        /// Increments the Active Directory context counter.
        /// </summary>
        public static void AddADContext()
        {
            ADContextCount++;
        }
        
        /// <summary>
        /// Decrements the Active Directory context counter.
        /// Ensures the count never goes below zero.
        /// </summary>
        public static void RemoveADContext()
        {
            if (ADContextCount > 0)
            {
                ADContextCount--;
            }
        }
        
        /// <summary>
        /// Gets the current count of active LDAP connections.
        /// </summary>
        public static int LdapConnectionCount => LdapConnections.Count;
        
        /// <summary>
        /// Internal collection tracking active LDAP connection GUIDs.
        /// </summary>
        private static readonly List<Guid> LdapConnections = new ();
        
        /// <summary>
        /// Removes an LDAP connection from tracking by its unique identifier.
        /// </summary>
        /// <param name="guid">The unique identifier of the LDAP connection to remove.</param>
        public static void RemoveLdapConnection(Guid guid)
        {
            LdapConnections.Remove(guid);

            OnLdapCountChanged.Invoke(LdapConnections.Count);
        }
        
        /// <summary>
        /// Adds a new LDAP connection to tracking with its unique identifier.
        /// </summary>
        /// <param name="guid">The unique identifier of the LDAP connection to add.</param>
        public static void AddLdapConnection(Guid guid)
        {
            LdapConnections.Add(guid);
            OnLdapCountChanged.Invoke(LdapConnections.Count);
        }

        /// <summary>
        /// Gets the current count of active database contexts.
        /// </summary>
        public static int DBContextCount { get; private set; }
        
        /// <summary>
        /// Increments the database context counter.
        /// </summary>
        public static void AddDBContext()
        {
            DBContextCount++;
        }
        
        /// <summary>
        /// Decrements the database context counter.
        /// Ensures the count never goes below zero.
        /// </summary>
        public static void RemoveDBContext()
        {
            if (DBContextCount > 0)
            {
                DBContextCount--;
            }
        }
        
        /// <summary>
        /// Rolling average of memory usage in bytes over the last 5 samples.
        /// </summary>
        public static RollingAverage MemoryUsage { get; private set; } = new(5);
        
        /// <summary>
        /// Rolling average of CPU usage percentage over the last 5 samples.
        /// </summary>
        public static RollingAverage CPUUsage { get; private set; } = new(5);
        
        /// <summary>
        /// Timer responsible for periodic resource usage polling.
        /// </summary>
        private static Timer? _resourceUsageTimer;
        
        /// <summary>
        /// Starts periodic polling of resource usage metrics (CPU and memory).
        /// Polls every 2 seconds if not already running.
        /// </summary>
        public static void StartResourceUsagePolling()
        {
            if (_resourceUsageTimer == null)
            {
                // Poll every 2000ms (2 seconds)
                _resourceUsageTimer = new Timer((state) => { PollData(); }, null, 0, 2000);
            }
        }
        
        /// <summary>
        /// Stops the resource usage polling timer and releases its resources.
        /// </summary>
        public static async Task StopResourceUsagePolling()
        {
            if (_resourceUsageTimer != null)
            {
                await _resourceUsageTimer.DisposeAsync();
                _resourceUsageTimer = null;
            }
        }
        
        /// <summary>
        /// Collects current resource usage data and updates rolling averages.
        /// Called periodically by the resource usage timer.
        /// </summary>
        private static void PollData()
        {
            try
            {
                // Refresh process data to get current values
                Process.Refresh();
                
                // Track memory usage (working set in bytes)
                MemoryUsage.AddValue(Process.WorkingSet64);
                
                // Calculate CPU usage as percentage
                var processTimeDelta = Process.TotalProcessorTime - lastProcessTime;
                lastProcessTime = Process.TotalProcessorTime;
                
                var pollingDelta = DateTime.Now - lastPollTime;
                var cpuValue = (processTimeDelta / pollingDelta) * 100;
                
                // Only add valid CPU values (0-100%)
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
        
        /// <summary>
        /// The total processor time from the last polling cycle, used to calculate CPU delta.
        /// </summary>
        private static TimeSpan lastProcessTime = TimeSpan.Zero;
        
        /// <summary>
        /// The timestamp of the last polling cycle, used to calculate time delta.
        /// </summary>
        private static DateTime lastPollTime = DateTime.Now;
    }
}
