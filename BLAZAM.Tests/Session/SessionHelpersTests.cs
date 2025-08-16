using BLAZAM.Helpers;
using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;

namespace BLAZAM.Tests.Session
{

    public class SessionHelpersTests
    {
        [Fact]
        public void IsAdminOrDemo_ReturnsTrue_ForAdminUsername()
        {
            var mockState = new Mock<IApplicationUserState>();
            mockState.Setup(s => s.Username).Returns("admin");
            Assert.True(mockState.Object.IsAdminOrDemo());
        }

        [Fact]
        public void IsAdminOrDemo_ReturnsTrue_ForDemoUsername()
        {
            var mockState = new Mock<IApplicationUserState>();
            mockState.Setup(s => s.Username).Returns("demo");
            Assert.True(mockState.Object.IsAdminOrDemo());
        }

        [Fact]
        public void IsAdminOrDemo_ReturnsFalse_ForOtherUsername()
        {
            var mockState = new Mock<IApplicationUserState>();
            mockState.Setup(s => s.Username).Returns("user");
            Assert.False(mockState.Object.IsAdminOrDemo());
        }

        [Fact]
        public void IsAdminOrDemo_ReturnsFalse_ForNullState()
        {
            IApplicationUserState? state = null;
            Assert.False(state.IsAdminOrDemo());
        }

        [Fact]
        public void IsAdminOrDemo_ReturnsFalse_ForNullUsername()
        {
            var mockState = new Mock<IApplicationUserState>();
            mockState.Setup(s => s.Username).Returns((string?)null);
            Assert.False(mockState.Object.IsAdminOrDemo());
        }

        [Fact]
        public void SlideCookieExpiration_DoesNotThrow_WhenHttpContextIsNull()
        {
            HttpContext? context = null;
            Exception? ex = Record.Exception(() => context.SlideCookieExpiration());
            Assert.Null(ex);
        }

        [Fact]
        public void GetSessionTimeout_ReturnsNull_WhenHttpContextIsNull()
        {
            HttpContext? context = null;
            var result = context.GetSessionTimeout();
            Assert.Null(result);
        }

        [Fact]
        public void GetSessionTimeout_ReturnsNull_WhenNoCookie()
        {
            var context = new DefaultHttpContext();
            var result = context.GetSessionTimeout();
            Assert.Null(result);
        }

        [Fact]
        public void GetAuthenticationCookie_ReturnsNull_WhenNoCookie()
        {
            var context = new DefaultHttpContext();
            var method = typeof(SessionHelpers)
                .GetMethod("GetAuthenticationCookie", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.NotNull(method); // Ensure method is found before invoking
            if (method != null)
            {
                var result = method?.Invoke(null, new object[] { context });
                Assert.Null(result);
            }
            throw new Exception("GetAuthenticationCookie method not found");
        }
    }
}