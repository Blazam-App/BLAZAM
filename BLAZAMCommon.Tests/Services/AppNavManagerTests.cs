using BLAZAM.Common.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace BLAZAMCommon.Tests.Services
{
    public class AppNavManagerTests
    {
        private class TestNavigationManager : NavigationManager
        {
            public TestNavigationManager(string baseUri = "https://example.com/", string uri = "https://example.com/test")
            {
                Initialize(baseUri, uri);
            }

            public void TriggerLocationChanged(string uri, bool isInterceptedLink)
            {
                Uri = uri;
                NotifyLocationChanged(isInterceptedLink);
            }

            protected override void NavigateToCore(string uri, NavigationOptions options)
            {
                Uri = uri;
            }
        }

        [Fact]
        public void Constructor_WithNavigationManager_ShouldInitialize()
        {
            // Arrange
            var navManager = new TestNavigationManager();

            // Act
            var appNavManager = new AppNavManager(navManager);

            // Assert
            Assert.NotNull(appNavManager);
        }

        [Fact]
        public void Uri_ShouldReturnNavigationManagerUri()
        {
            // Arrange
            var expectedUri = "https://example.com/test";
            var navManager = new TestNavigationManager(uri: expectedUri);
            var appNavManager = new AppNavManager(navManager);

            // Act
            var actualUri = appNavManager.Uri;

            // Assert
            Assert.Equal(expectedUri, actualUri);
        }

        [Fact]
        public void BaseUri_ShouldReturnNavigationManagerBaseUri()
        {
            // Arrange
            var expectedBaseUri = "https://example.com/";
            var navManager = new TestNavigationManager(baseUri: expectedBaseUri);
            var appNavManager = new AppNavManager(navManager);

            // Act
            var actualBaseUri = appNavManager.BaseUri;

            // Assert
            Assert.Equal(expectedBaseUri, actualBaseUri);
        }

        [Fact]
        public void LocationChanged_AddHandler_ShouldReceiveEvents()
        {
            // Arrange
            var navManager = new TestNavigationManager();
            var appNavManager = new AppNavManager(navManager);
            var eventFired = false;
            var eventArgs = default(LocationChangedEventArgs);

            // Act
            appNavManager.LocationChanged += (sender, args) =>
            {
                eventFired = true;
                eventArgs = args;
            };
            navManager.TriggerLocationChanged("https://example.com/newpage", false);

            // Assert
            Assert.True(eventFired);
            Assert.NotNull(eventArgs);
        }

        [Fact]
        public void LocationChanged_RemoveHandler_ShouldNotReceiveEvents()
        {
            // Arrange
            var navManager = new TestNavigationManager();
            var appNavManager = new AppNavManager(navManager);
            var eventFired = false;
            EventHandler<LocationChangedEventArgs> handler = (sender, args) =>
            {
                eventFired = true;
            };

            // Act
            appNavManager.LocationChanged += handler;
            appNavManager.LocationChanged -= handler;
            navManager.TriggerLocationChanged("https://example.com/newpage", false);

            // Assert
            Assert.False(eventFired);
        }

        [Fact]
        public void ToBaseRelativePath_ShouldForwardToNavigationManager()
        {
            // Arrange
            var baseUri = "https://example.com/";
            var inputUri = "https://example.com/path/to/page";
            var navManager = new TestNavigationManager(baseUri: baseUri);
            var appNavManager = new AppNavManager(navManager);

            // Act
            var actualRelativePath = appNavManager.ToBaseRelativePath(inputUri);

            // Assert
            Assert.Equal("path/to/page", actualRelativePath);
        }

        [Theory]
        [InlineData("https://example.com/", "https://example.com/page1", "page1")]
        [InlineData("https://example.com/", "https://example.com/", "")]
        [InlineData("https://example.com/", "https://example.com/nested/path", "nested/path")]
        public void ToBaseRelativePath_WithVariousUris_ShouldReturnExpectedPaths(string baseUri, string inputUri, string expectedPath)
        {
            // Arrange
            var navManager = new TestNavigationManager(baseUri: baseUri);
            var appNavManager = new AppNavManager(navManager);

            // Act
            var actualPath = appNavManager.ToBaseRelativePath(inputUri);

            // Assert
            Assert.Equal(expectedPath, actualPath);
        }

        [Fact]
        public void ToAbsoluteUri_WithRelativeUri_ShouldReturnAbsoluteUri()
        {
            // Arrange
            var baseUri = "https://example.com/";
            var relativeUri = "page/test";
            var navManager = new TestNavigationManager(baseUri: baseUri);
            var appNavManager = new AppNavManager(navManager);

            // Act
            var result = appNavManager.ToAbsoluteUri(relativeUri);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("https://example.com/page/test", result.ToString());
        }

        [Theory]
        [InlineData("relative/path", "relative/path")]
        [InlineData("/absolute/path", "/absolute/path")]
        [InlineData("", "")]
        public void ToAbsoluteUri_WithVariousRelativeUris_ShouldReturnAbsoluteUri(string relativeUri, string expectedPath)
        {
            // Arrange
            var baseUri = "https://example.com/";
            var navManager = new TestNavigationManager(baseUri: baseUri);
            var appNavManager = new AppNavManager(navManager);

            // Act
            var result = appNavManager.ToAbsoluteUri(relativeUri);

            // Assert
            Assert.NotNull(result);
            Assert.Contains(expectedPath, result.ToString());
        }

        [Fact]
        public void NavigateTo_WithUri_ShouldCallNavigationManager()
        {
            // Arrange
            var navManager = new TestNavigationManager();
            var appNavManager = new AppNavManager(navManager);
            var targetUri = "https://example.com/target";

            // Act
            appNavManager.NavigateTo(targetUri);

            // Assert
            Assert.Equal(targetUri, navManager.Uri);
        }

        [Theory]
        [InlineData("https://example.com/page1", false, false)]
        [InlineData("https://example.com/page2", true, false)]
        [InlineData("https://example.com/page3", false, true)]
        [InlineData("https://example.com/page4", true, true)]
        public void NavigateTo_WithBoolParameters_ShouldCallNavigationManager(string uri, bool forceLoad, bool replace)
        {
            // Arrange
            var navManager = new TestNavigationManager();
            var appNavManager = new AppNavManager(navManager);

            // Act
            appNavManager.NavigateTo(uri, forceLoad, replace);

            // Assert
            Assert.Equal(uri, navManager.Uri);
        }

        [Fact]
        public void NavigateTo_WithNavigationOptions_ShouldCallNavigationManager()
        {
            // Arrange
            var navManager = new TestNavigationManager();
            var appNavManager = new AppNavManager(navManager);
            var targetUri = "https://example.com/target";
            var options = new NavigationOptions { ForceLoad = true, ReplaceHistoryEntry = true };

            // Act
            appNavManager.NavigateTo(targetUri, options);

            // Assert
            Assert.Equal(targetUri, navManager.Uri);
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public void NavigateTo_WithVariousNavigationOptions_ShouldCallNavigationManager(bool forceLoad, bool replaceHistory)
        {
            // Arrange
            var navManager = new TestNavigationManager();
            var appNavManager = new AppNavManager(navManager);
            var targetUri = "https://example.com/target";
            var options = new NavigationOptions { ForceLoad = forceLoad, ReplaceHistoryEntry = replaceHistory };

            // Act
            appNavManager.NavigateTo(targetUri, options);

            // Assert
            Assert.Equal(targetUri, navManager.Uri);
        }
    }
}
