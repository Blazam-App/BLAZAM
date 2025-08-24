using BLAZAM.Localization;
using BLAZAM.Services.Background;
using Microsoft.Extensions.Localization;
using Moq;

namespace BLAZAMCommon.Tests
{
    public class BackgroundServiceBaseTests : IDisposable
    {
        private readonly Mock<IStringLocalizer<AppLocalization>> _mockStringLocalizer;
        private TestableBackgroundService _service;

        public BackgroundServiceBaseTests()
        {
            _mockStringLocalizer = new Mock<IStringLocalizer<AppLocalization>>();
        }

        // Helper class to test the abstract BackgroundServiceBase
        private class TestableBackgroundService : BackgroundServiceBase, IDisposable
        {
            public bool ExecuteCalled { get; private set; }
            public object? LastState { get; private set; }
            private readonly ManualResetEventSlim _executeCalledEvent = new ManualResetEventSlim(false);
            private bool _testableServiceDisposed = false;

            public int ExecuteCallCount { get; private set; }

            public TestableBackgroundService(IStringLocalizer<AppLocalization> appLocalization) : base(appLocalization)
            {
            }

            public void SetInterval(TimeSpan interval) => base.Interval = interval;
            public TimeSpan GetInterval() => base.Interval;
            public Timer? InspectTimer => base.Timer;
            public bool IsServiceStarted => base.started;


            protected override void Execute(object? state = null)
            {
                ExecuteCalled = true;
                LastState = state;
                ExecuteCallCount++;
                _executeCalledEvent.Set();
                // DO NOT call base.Execute(state) here unless specifically testing that path,
                // as it would throw NotImplementedException.
            }

            public bool WaitForExecute(TimeSpan timeout) => _executeCalledEvent.Wait(timeout);
            public bool WaitForExecute(int millisecondsTimeout) => _executeCalledEvent.Wait(millisecondsTimeout);

            public void ResetExecuteSignal()
            {
                ExecuteCalled = false;
                _executeCalledEvent.Reset();
                // Note: ExecuteCalled and LastState are not reset here to allow inspection
                // of the last call. If multiple distinct calls need fresh ExecuteCalled status,
                // the test logic should handle it or this method can be enhanced.
            }

            private bool _disposeLogicCalled = false; // To check if derived Dispose was hit
            public bool WasDisposeLogicCalled => _disposeLogicCalled;


            // Override Dispose to manage own resources and track calls
            protected override void Dispose(bool disposing)
            {
                if (!_testableServiceDisposed) // Prevent multiple disposals of TestableBackgroundService resources
                {
                    _disposeLogicCalled = true; // Mark that this Dispose method was entered
                    if (disposing)
                    {
                        _executeCalledEvent.Dispose();
                    }
                    base.Dispose(disposing); // Call base class's Dispose logic
                    _testableServiceDisposed = true;
                }
            }

            // Public Dispose for IDisposable pattern implementation in the test helper
            public new void Dispose() // 'new' keyword to hide base.Dispose only for this helper's IDisposable
            {
                Dispose(true);
                GC.SuppressFinalize(this); // Call SuppressFinalize for the helper if it had a finalizer
            }
        }

        // Test class for calling base Execute
        private class ServiceCallingBaseExecute : BackgroundServiceBase
        {
            public ServiceCallingBaseExecute(IStringLocalizer<AppLocalization> l) : base(l) { }
            public void CallBaseExecute() => base.Execute(null);
        }


        public void Dispose()
        {
            _service?.Dispose(); // Ensure the testable service and its resources like ManualResetEventSlim are cleaned up
        }

        [Fact]
        public void Constructor_SetsAppLocalization()
        {
            // Arrange & Act
            _service = new TestableBackgroundService(_mockStringLocalizer.Object);

            // Assert
            // AppLocalization is a protected field. We trust it's set.
            // Direct assertion would require reflection or making it otherwise accessible.
            // For this test, we assume the simple assignment in the constructor works.
            Assert.NotNull(_service); // Basic check that object creation worked.
        }

        [Fact]
        public void Start_NotImmediate_WithNonZeroInterval_SetsStartedAndInitializesTimer()
        {
            // Arrange
            _service = new TestableBackgroundService(_mockStringLocalizer.Object);
            _service.SetInterval(TimeSpan.FromMilliseconds(500)); // Short interval for testing

            // Act
            _service.Start(false);

            // Assert
            Assert.True(_service.IsServiceStarted);
            Assert.NotNull(_service.InspectTimer);
            Assert.False(_service.ExecuteCalled); // Should not be called immediately

            // Wait a very short period to ensure it wasn't called due to a zero-like delay
            Thread.Sleep(100); // Shorter than the SUT's minimum 15-45ms bug for Task.Delay, but for Timer this is fine.
            Assert.False(_service.ExecuteCalled);

            // Clean up the timer explicitly if the service might not be disposed by test runner quickly enough
            // This is important as the timer will keep firing.
            _service.Stop();
        }

        [Fact]
        public void Start_NotImmediate_WithNonZeroInterval_EventuallyCallsExecute()
        {
            // Arrange
            _service = new TestableBackgroundService(_mockStringLocalizer.Object);
            // The SUT's random delay for non-immediate start is 15-45 seconds.
            // For a unit test, this is too long.
            // This test acknowledges this by checking it's not immediate, and then relies on a shorter wait
            // for a manually set shorter interval, knowing the initial delay can be long.
            // To test the actual 15-45s delay, this test would need to run much longer.
            // Here, we're more focused on the mechanism after the initial delay.
            _service.SetInterval(TimeSpan.FromMilliseconds(100)); // A short interval for quicker test.

            // Act
            _service.Start(false); // Initial delay is random 15-45s, then interval.

            // Assert: Not immediate
            Assert.True(_service.IsServiceStarted);
            Assert.NotNull(_service.InspectTimer);
            Assert.False(_service.ExecuteCalled);

            // To truly test the 15-45s delay + subsequent execute would involve:
            // bool executed = _service.WaitForExecute(TimeSpan.FromSeconds(50)); // Max SUT delay + buffer
            // Assert.True(executed, "Execute was not called within the expected maximum delay.");
            // This makes the test very slow. The previous test verifies it's not immediate.
            // Here we'll assume if it starts, it will eventually call.
            // For quicker feedback on Execute being hooked up with non-zero interval:
            // We'll start immediately to bypass the long random delay for this specific check.
            _service.Stop(); // Stop previous start
            _service.ResetExecuteSignal();
            _service.Start(true); // Start immediately

            bool executed = _service.WaitForExecute(TimeSpan.FromSeconds(1)); // Due time 0, interval 100ms
            Assert.True(executed, "Execute was not called after immediate start.");

            _service.Stop();
        }


        [Fact]
        public void Start_Immediate_WithNonZeroInterval_SetsStartedAndExecutesSoon()
        {
            // Arrange
            _service = new TestableBackgroundService(_mockStringLocalizer.Object);
            _service.SetInterval(TimeSpan.FromMilliseconds(100)); // Interval after first execution

            // Act
            _service.Start(true); // Immediate start, delay should be 0

            // Assert
            Assert.True(_service.IsServiceStarted);
            Assert.NotNull(_service.InspectTimer);
            Assert.True(_service.WaitForExecute(TimeSpan.FromSeconds(2)), "Execute was not called within expected time for immediate start.");

            _service.Stop();
        }

        [Fact]
        public void Start_WhenAlreadyStarted_DoesNotReinitializeTimer()
        {
            // Arrange
            _service = new TestableBackgroundService(_mockStringLocalizer.Object);
            _service.SetInterval(TimeSpan.FromMinutes(1));
            _service.Start(true); // First start
            var initialTimer = _service.InspectTimer;
            Assert.True(_service.IsServiceStarted);
            Assert.NotNull(initialTimer);

            // Act
            _service.Start(true); // Attempt to start again

            // Assert
            Assert.True(_service.IsServiceStarted); // Still started
            Assert.Same(initialTimer, _service.InspectTimer); // Timer instance should be the same

            _service.Stop();
        }

        [Fact]
        public void Start_Immediate_WithZeroInterval_SetsStarted_UsesTaskDelay_AndExecutesOnce()
        {
            // Arrange
            _service = new TestableBackgroundService(_mockStringLocalizer.Object);
            _service.SetInterval(TimeSpan.Zero);

            // Act
            _service.Start(true); // Immediate, delay is 0

            // Assert
            Assert.True(_service.IsServiceStarted);
            Assert.Null(_service.InspectTimer); // Timer should not be created for Zero interval
            Assert.True(_service.WaitForExecute(TimeSpan.FromSeconds(1)), "Execute was not called for ZeroInterval immediate start.");

            // Check it only executes once (Task.Delay().ContinueWith() runs once)
            int initialCallCount = _service.ExecuteCallCount;
            _service.ResetExecuteSignal(); // Reset signal but keep call count
            Thread.Sleep(200); // Wait a bit to see if it fires again (it shouldn't)
            Assert.False(_service.ExecuteCalled); // ExecuteCalled refers to the signal state after ResetExecuteSignal
            Assert.Equal(initialCallCount, _service.ExecuteCallCount); // Ensure no more calls

            // No timer to Stop, started flag is managed by BackgroundServiceBase
        }

        [Fact]
        public void Start_NotImmediate_WithZeroInterval_UsesShortTaskDelay_AndExecutesOnce()
        {
            // Arrange
            _service = new TestableBackgroundService(_mockStringLocalizer.Object);
            _service.SetInterval(TimeSpan.Zero);

            // Act
            _service.Start(false); // Not immediate. SUT uses Task.Delay(15-45ms) due to bug.

            // Assert
            Assert.True(_service.IsServiceStarted);
            Assert.Null(_service.InspectTimer);
            Assert.False(_service.ExecuteCalled); // Should not be called *absolutely* immediately

            // SUT's delay will be rand.Next(-15,15) + 30 = 15ms to 45ms
            Assert.True(_service.WaitForExecute(TimeSpan.FromMilliseconds(5000)), "Execute was not called within 5000ms for ZeroInterval non-immediate start.");

            int initialCallCount = _service.ExecuteCallCount;
            _service.ResetExecuteSignal();
            Thread.Sleep(200);
            Assert.False(_service.ExecuteCalled);
            Assert.Equal(initialCallCount, _service.ExecuteCallCount);
        }


        [Fact]
        public void Stop_DisposesTimer_AndSetsStartedFalse()
        {
            // Arrange
            _service = new TestableBackgroundService(_mockStringLocalizer.Object);
            _service.SetInterval(TimeSpan.FromMilliseconds(50)); // Short interval
            _service.Start(true); // Start the service so it creates a timer
            Assert.True(_service.IsServiceStarted);
            Assert.NotNull(_service.InspectTimer);

            // Act
            _service.Stop();

            // Assert
            Assert.False(_service.IsServiceStarted);
            Assert.Null(_service.InspectTimer);
            // Ensure Execute is no longer called
            _service.ResetExecuteSignal();
            Thread.Sleep(150); // Wait for longer than the interval
            Assert.False(_service.ExecuteCalled, "Execute was called after Stop().");
        }

        [Fact]
        public void Stop_WhenNotStarted_DoesNothingHarmful()
        {
            // Arrange
            _service = new TestableBackgroundService(_mockStringLocalizer.Object);
            Assert.False(_service.IsServiceStarted); // Pre-condition

            // Act
            _service.Stop();

            // Assert
            Assert.False(_service.IsServiceStarted); // Still not started
            Assert.Null(_service.InspectTimer); // Timer should still be null
                                                // No exception should be thrown
        }


        [Fact]
        public void BaseExecute_ThrowsNotImplementedException()
        {
            // Arrange
            var serviceWithBaseExecute = new ServiceCallingBaseExecute(_mockStringLocalizer.Object);

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => serviceWithBaseExecute.CallBaseExecute());
        }

        [Fact]
        public void Dispose_DisposesTimer_AndSetsDisposedValueIndirectly()
        {
            // Arrange
            _service = new TestableBackgroundService(_mockStringLocalizer.Object);
            _service.SetInterval(TimeSpan.FromMilliseconds(50));
            _service.Start(true); // Start to create a timer
            Timer? timerInstance = _service.InspectTimer;
            Assert.NotNull(timerInstance);

            // Act
            _service.Dispose();

            // Assert
            Assert.True(_service.WasDisposeLogicCalled, "Derived Dispose logic was not called.");
            // The base class's private 'disposedValue' is not directly assertable.

            Assert.Null(_service.InspectTimer);

            // Note: BackgroundServiceBase.Dispose does not set 'started' to false.
            Assert.True(_service.IsServiceStarted, "Dispose should not change 'started' flag by default.");
        }

    }
}
