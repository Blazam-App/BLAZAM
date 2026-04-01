namespace BLAZAMCommon.Tests // You can adjust the namespace to fit your test project structure
{
    public class AppEventTests
    {
        [Fact]
        public void Invoke_ShouldCallDelegate_WhenDelegateIsSet()
        {
            // Arrange
            var appEvent = new AppEvent();
            bool delegateCalled = false;
            object capturedSender = new object(); // Placeholder, will be null
            EventArgs? capturedArgs = null;

            appEvent.Delegate += (sender, args) =>
            {
                delegateCalled = true;
                capturedSender = sender;
                capturedArgs = args;
            };

            // Act
            appEvent.Invoke();

            // Assert
            Assert.True(delegateCalled, "Delegate should have been called.");
            Assert.Null(capturedSender); // As per Invoke(), sender is null
            Assert.Same(EventArgs.Empty, capturedArgs); // As per Invoke(), args is EventArgs.Empty
        }

        [Fact]
        public void Invoke_ShouldNotThrowException_WhenDelegateIsNull()
        {
            // Arrange
            var appEvent = new AppEvent();
            // appEvent.Delegate = null; // Ensure delegate is null

            // Act
            var exception = Record.Exception(() => appEvent.Invoke());

            // Assert
            Assert.Null(exception); // No exception should be thrown
        }


        private class TestEventArgs : EventArgs // Custom EventArgs for testing
        {
            public string Message { get; set; }
        }

        [Fact]
        public void Invoke_WithSenderAndArgs_ShouldCallDelegate_WhenDelegateIsSet()
        {
            // Arrange
            var appEvent = new AppEvent<TestEventArgs>();
            bool delegateCalled = false;
            object expectedSender = new object();
            var expectedArgs = new TestEventArgs { Message = "Hello Test" };
            object capturedSender = null;
            TestEventArgs capturedArgs = null;

            appEvent.Delegate += (sender, args) =>
            {
                delegateCalled = true;
                capturedSender = sender;
                capturedArgs = args;
            };

            // Act
            appEvent.Invoke(expectedSender, expectedArgs);

            // Assert
            Assert.True(delegateCalled, "Delegate should have been called.");
            Assert.Same(expectedSender, capturedSender);
            Assert.Same(expectedArgs, capturedArgs);
            Assert.Equal("Hello Test", capturedArgs.Message);
        }

        [Fact]
        public void Invoke_WithArgsOnly_ShouldCallDelegate_WhenDelegateIsSet()
        {
            // Arrange
            var appEvent = new AppEvent<TestEventArgs>();
            bool delegateCalled = false;
            var expectedArgs = new TestEventArgs { Message = "Hello Args Only" };
            object capturedSender = new object(); // Placeholder, will be null
            TestEventArgs capturedArgs = null;

            appEvent.Delegate += (sender, args) =>
            {
                delegateCalled = true;
                capturedSender = sender;
                capturedArgs = args;
            };

            // Act
            appEvent.Invoke(expectedArgs);

            // Assert
            Assert.True(delegateCalled, "Delegate should have been called.");
            Assert.Null(capturedSender); // As per Invoke(args), sender is null
            Assert.Same(expectedArgs, capturedArgs);
            Assert.Equal("Hello Args Only", capturedArgs.Message);
        }

        [Fact]
        public void Invoke_WithSenderAndArgs_ShouldNotThrowException_WhenDelegateIsNull()
        {
            // Arrange
            var appEvent = new AppEvent<TestEventArgs>
            {
                Delegate = null // Ensure delegate is null
            };
            object sender = new object();
            var args = new TestEventArgs { Message = "Test" };

            // Act
            var exception = Record.Exception(() => appEvent.Invoke(sender, args));

            // Assert
            Assert.Null(exception); // No exception should be thrown
        }

        [Fact]
        public void Invoke_WithArgsOnly_ShouldNotThrowException_WhenDelegateIsNull()
        {
            // Arrange
            var appEvent = new AppEvent<TestEventArgs>
            {
                Delegate = null // Ensure delegate is null
            };
            var args = new TestEventArgs { Message = "Test" };

            // Act
            var exception = Record.Exception(() => appEvent.Invoke(args));

            // Assert
            Assert.Null(exception); // No exception should be thrown
        }

        [Fact]
        public void Delegate_CanBeSubscribedAndUnsubscribed()
        {
            // Arrange
            var appEvent = new AppEvent<string>();
            int callCount = 0;
            EventHandler<string> handler1 = (s, a) => callCount++;
            EventHandler<string> handler2 = (s, a) => callCount++;

            // Act & Assert for subscription
            appEvent.Delegate += handler1;
            appEvent.Delegate += handler2;
            appEvent.Invoke("test1");
            Assert.Equal(2, callCount); // Both handlers should be called

            // Act & Assert for unsubscription
            callCount = 0; // Reset counter
            appEvent.Delegate -= handler1;
            appEvent.Invoke("test2");
            Assert.Equal(1, callCount); // Only handler2 should be called

            callCount = 0; // Reset counter
            appEvent.Delegate -= handler2;
            appEvent.Invoke("test3");
            Assert.Equal(0, callCount); // No handlers should be called
        }
    }

}