using BLAZAM.Common.Data;
using Moq;
using System.Diagnostics;
using System.Reflection;

namespace BLAZAMCommon.Tests.Data
{
    public class ApplicationStatisticsTests : IDisposable
    {
        private readonly Mock<Process> _mockProcess;
        private readonly Process _originalProcess;

        // Helper method to reset private static timer fields for PollData tests
        private static void ResetPollDataStaticTimers(DateTime pollTime, TimeSpan processTime)
        {
            SetStaticField("lastPollTime", pollTime);
            SetStaticField("lastProcessTime", processTime);
        }

        // Helper method to get private static fields
        private static T GetStaticField<T>(string fieldName)
        {
            FieldInfo field = typeof(ApplicationStatistics).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
            if (field == null) throw new FieldAccessException($"Static field '{fieldName}' not found in ApplicationStatistics.");
            return (T)field.GetValue(null);
        }

        // Helper method to set private static fields
        private static void SetStaticField(string fieldName, object value)
        {
            FieldInfo field = typeof(ApplicationStatistics).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
            if (field == null) throw new FieldAccessException($"Static field '{fieldName}' not found in ApplicationStatistics.");
            field.SetValue(null, value);
        }


        public ApplicationStatisticsTests()
        {
            _mockProcess = new Mock<Process>();
            _originalProcess = ApplicationStatistics.Process; // Save current process
            ApplicationStatistics.Process = _mockProcess.Object; // Set our mock process

            // Reset counts before each test
            while (ApplicationStatistics.ADContextCount > 0)
            {
                ApplicationStatistics.RemoveADContext();
            }
            while (ApplicationStatistics.DBContextCount > 0)
            {
                ApplicationStatistics.RemoveDBContext();
            }

            // Stop any existing timer from previous tests (if any test failed to clean up)
            // and reset the timer field via reflection.
            var timerField = typeof(ApplicationStatistics).GetField("_resourceUsageTimer", BindingFlags.NonPublic | BindingFlags.Static);
            var existingTimer = timerField?.GetValue(null) as System.Threading.Timer;
            existingTimer?.Dispose();
            timerField?.SetValue(null, null);

            // Note: RollingAverage instances (MemoryUsage, CPUUsage) are harder to reset
            // as they are initialized with `new(5)` and have private setters.
            // Their state might persist across tests if PollData is called.
            // Tests for PollData will focus on interactions and input to RollingAverage.
        }

        public void Dispose()
        {
            // Restore the original process
            ApplicationStatistics.Process = _originalProcess;

            // Attempt to stop and dispose of the timer if it was started
            FieldInfo timerField = typeof(ApplicationStatistics).GetField("_resourceUsageTimer", BindingFlags.NonPublic | BindingFlags.Static);
            var timer = (System.Threading.Timer)timerField?.GetValue(null);
            if (timer != null)
            {
                ApplicationStatistics.StopResourceUsagePolling().GetAwaiter().GetResult();
            }
            // Explicitly null out via reflection to ensure clean state for next test,
            // as StopResourceUsagePolling might be slow or StopAsync has nuances in test runners.
            timerField?.SetValue(null, null);


            // Reset static time fields to their defaults after tests that manipulate them
            SetStaticField("lastProcessTime", TimeSpan.Zero);
            SetStaticField("lastPollTime", DateTime.Now); // Or a fixed known date if more predictability is needed across all tests.
                                                          // DateTime.Now is what the class sets initially.
        }

        [Fact]
        public void AddADContext_IncrementsCount()
        {
            ApplicationStatistics.AddADContext();
            Assert.Equal(1, ApplicationStatistics.ADContextCount);

            ApplicationStatistics.AddADContext();
            Assert.Equal(2, ApplicationStatistics.ADContextCount);
        }

        [Fact]
        public void RemoveADContext_DecrementsCount()
        {
            ApplicationStatistics.AddADContext();
            ApplicationStatistics.AddADContext();

            ApplicationStatistics.RemoveADContext();
            Assert.Equal(1, ApplicationStatistics.ADContextCount);

            ApplicationStatistics.RemoveADContext();
            Assert.Equal(0, ApplicationStatistics.ADContextCount);
        }

        [Fact]
        public void RemoveADContext_WhenCountIsZero_DoesNotGoNegative()
        {
            Assert.Equal(0, ApplicationStatistics.ADContextCount); // Ensure it's zero
            ApplicationStatistics.RemoveADContext();
            Assert.Equal(0, ApplicationStatistics.ADContextCount);
        }

        [Fact]
        public void AddDBContext_IncrementsCount()
        {
            ApplicationStatistics.AddDBContext();
            Assert.Equal(1, ApplicationStatistics.DBContextCount);

            ApplicationStatistics.AddDBContext();
            Assert.Equal(2, ApplicationStatistics.DBContextCount);
        }

        [Fact]
        public void RemoveDBContext_DecrementsCount()
        {
            ApplicationStatistics.AddDBContext();
            ApplicationStatistics.AddDBContext();

            ApplicationStatistics.RemoveDBContext();
            Assert.Equal(1, ApplicationStatistics.DBContextCount);

            ApplicationStatistics.RemoveDBContext();
            Assert.Equal(0, ApplicationStatistics.DBContextCount);
        }

        [Fact]
        public void RemoveDBContext_WhenCountIsZero_DoesNotGoNegative()
        {
            Assert.Equal(0, ApplicationStatistics.DBContextCount); // Ensure it's zero
            ApplicationStatistics.RemoveDBContext();
            Assert.Equal(0, ApplicationStatistics.DBContextCount);
        }

        [Fact]
        public async Task StartResourceUsagePolling_InitializesAndCanBeStopped()
        {
            FieldInfo timerField = typeof(ApplicationStatistics).GetField("_resourceUsageTimer", BindingFlags.NonPublic | BindingFlags.Static);

            Assert.Null(timerField.GetValue(null)); // Timer should be null initially

            ApplicationStatistics.StartResourceUsagePolling();
            var timerInstance1 = timerField.GetValue(null);
            Assert.NotNull(timerInstance1);

            // Calling again should not create a new timer
            ApplicationStatistics.StartResourceUsagePolling();
            Assert.Same(timerInstance1, timerField.GetValue(null));

            await ApplicationStatistics.StopResourceUsagePolling();
            Assert.Null(timerField.GetValue(null)); // Timer should be disposed and nulled
        }


        [Fact]
        public async Task StopResourceUsagePolling_HandlesNullTimerGracefully()
        {
            FieldInfo timerField = typeof(ApplicationStatistics).GetField("_resourceUsageTimer", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.Null(timerField.GetValue(null)); // Ensure timer is initially null

            // Stopping a null/disposed timer should not throw
            await ApplicationStatistics.StopResourceUsagePolling();
            Assert.Null(timerField.GetValue(null)); // Still null
        }






    }
}
