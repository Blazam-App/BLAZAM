using System.Security.Claims;
using BLAZAM.Common.Data;
using BLAZAM.Helpers;
using Microsoft.AspNetCore.Components.Authorization;
using Moq;

namespace BLAZAMCommon.Tests.Data
{
    public class LoginRequestTests
    {
        [Fact]
        public void Id_ShouldBeInitializedOnCreation()
        {
            // Arrange & Act
            var loginRequest = new LoginRequest();

            // Assert
            Assert.NotEqual(Guid.Empty, loginRequest.Id);
        }

        [Fact]
        public void Constructor_DefaultValues_AreSetCorrectly()
        {
            // Arrange & Act
            var loginRequest = new LoginRequest();

            // Assert
            Assert.Null(loginRequest.Username);
            Assert.Null(loginRequest.Password);
            Assert.Null(loginRequest.SecurePassword);
            Assert.Equal("/", loginRequest.ReturnUrl);
            Assert.Null(loginRequest.CallbackBaseUri);
            Assert.False(loginRequest.Impersonation);
            Assert.Null(loginRequest.ImpersonatorClaims);
            Assert.Null(loginRequest.IPAddress);
            Assert.Null(loginRequest.MFAToken);
            Assert.Null(loginRequest.MFARedirect);
            Assert.Null(loginRequest.AuthenticationResult);
            Assert.Null(loginRequest.AuthenticationState);
        }

        [Fact]
        public void Username_SetAndGet_ShouldWork()
        {
            // Arrange
            var loginRequest = new LoginRequest();
            var testUsername = "testUser";

            // Act
            loginRequest.Username = testUsername;

            // Assert
            Assert.Equal(testUsername, loginRequest.Username);
        }

        [Theory]
        [InlineData("testPassword")]
        [InlineData("")]
        [InlineData(null)]
        public void Password_SetAndGet_ShouldHandleDifferentValues(string testPassword)
        {
            // Arrange
            var loginRequest = new LoginRequest();

            // Act
            loginRequest.Password = testPassword;
            var retrievedPassword = loginRequest.Password;
            var securePassword = loginRequest.SecurePassword;

            // Assert
            Assert.Equal(testPassword, retrievedPassword); // Check plain text retrieval

            if (testPassword == null)
            {
                Assert.Null(securePassword); // SecureString should be null
            }
            else
            {
                Assert.NotNull(securePassword); // SecureString should not be null
                // This relies on the BLAZAM.Helpers.ToPlainText extension method working correctly
                Assert.Equal(testPassword, securePassword.ToPlainText());
            }
        }

        [Fact]
        public void SecurePassword_ShouldReflectPasswordSet()
        {
            // Arrange
            var loginRequest = new LoginRequest();
            var testPassword = "securePassword123";

            // Act
            loginRequest.Password = testPassword;

            // Assert
            Assert.NotNull(loginRequest.SecurePassword);
            // This relies on the BLAZAM.Helpers.ToPlainText extension method working correctly
            Assert.Equal(testPassword, loginRequest.SecurePassword.ToPlainText());
        }

        [Fact]
        public void SecurePassword_WhenPasswordSetToNull_ShouldBeNull()
        {
            // Arrange
            var loginRequest = new LoginRequest();

            // Act
            loginRequest.Password = null;

            // Assert
            Assert.Null(loginRequest.SecurePassword);
        }

        [Fact]
        public void ReturnUrl_SetAndGet_ShouldWork()
        {
            // Arrange
            var loginRequest = new LoginRequest();
            var testReturnUrl = "/dashboard";

            // Act
            loginRequest.ReturnUrl = testReturnUrl;

            // Assert
            Assert.Equal(testReturnUrl, loginRequest.ReturnUrl);
        }

        [Fact]
        public void CallbackBaseUri_SetAndGet_ShouldWork()
        {
            // Arrange
            var loginRequest = new LoginRequest();
            var testUri = "https://example.com/callback";

            // Act
            loginRequest.CallbackBaseUri = testUri;

            // Assert
            Assert.Equal(testUri, loginRequest.CallbackBaseUri);
        }

        [Theory]
        [InlineData("user", "pass", true)]
        [InlineData("user", " ", true)]   // Password with only space (counts as non-empty)
        [InlineData("user", "", false)]   // Empty password
        [InlineData("user", null, false)] // Null password
        [InlineData("", "pass", false)]    // Empty username
        [InlineData(null, "pass", false)]  // Null username
        [InlineData("", "", false)]        // Empty username and password
        [InlineData(null, null, false)]    // Null username and password
        public void Valid_Property_ShouldReturnExpectedValue(string username, string password, bool expectedValid)
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = username,
                Password = password // Setter handles conversion to SecureString
            };

            // Act
            var isValid = loginRequest.Valid;

            // Assert
            Assert.Equal(expectedValid, isValid);
        }

        [Fact]
        public void Impersonation_SetAndGet_ShouldWork()
        {
            // Arrange
            var loginRequest = new LoginRequest();

            // Act
            loginRequest.Impersonation = true;

            // Assert
            Assert.True(loginRequest.Impersonation);

            loginRequest.Impersonation = false;
            Assert.False(loginRequest.Impersonation);
        }

        [Fact]
        public void ImpersonatorClaims_SetAndGet_ShouldWork()
        {
            // Arrange
            var loginRequest = new LoginRequest();
            var claimsIdentity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "impersonator") });
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // Act
            loginRequest.ImpersonatorClaims = claimsPrincipal;

            // Assert
            Assert.Same(claimsPrincipal, loginRequest.ImpersonatorClaims);
        }

        [Fact]
        public void IPAddress_SetAndGet_ShouldWork()
        {
            // Arrange
            var loginRequest = new LoginRequest();
            var testIp = "192.168.1.1";

            // Act
            loginRequest.IPAddress = testIp;

            // Assert
            Assert.Equal(testIp, loginRequest.IPAddress);
        }

        [Fact]
        public void MFAToken_SetAndGet_ShouldWork()
        {
            // Arrange
            var loginRequest = new LoginRequest();
            var testToken = "mfaToken123";

            // Act
            loginRequest.MFAToken = testToken;

            // Assert
            Assert.Equal(testToken, loginRequest.MFAToken);
        }

        [Fact]
        public void MFARedirect_SetAndGet_ShouldWork()
        {
            // Arrange
            var loginRequest = new LoginRequest();
            var testRedirect = "/mfa-verify";

            // Act
            loginRequest.MFARedirect = testRedirect;

            // Assert
            Assert.Equal(testRedirect, loginRequest.MFARedirect);
        }

        // --- Tests for state-changing methods ---

        [Fact]
        public void UnauthorizedImpersonation_ShouldSetResultAndReturnInstance()
        {
            // Arrange
            var loginRequest = new LoginRequest();

            // Act
            var result = loginRequest.UnauthorizedImpersonation();

            // Assert
            Assert.Equal(LoginResultStatus.UnauthorizedImpersonation, loginRequest.AuthenticationResult);
            Assert.Same(loginRequest, result); // Check for fluent return
        }

        [Fact]
        public void DuoRequested_ShouldSetResultAndStateAndReturnInstance()
        {
            // Arrange
            var loginRequest = new LoginRequest();
            // AuthenticationState is a concrete class from Microsoft.AspNetCore.Components.Authorization
            // If it had complex dependencies or logic, we might mock its constructor args or use a more involved Moq setup.
            // For this test, simply passing a new instance or a simple mock is usually sufficient.
            var mockUser = new ClaimsPrincipal(new ClaimsIdentity()); // Minimal ClaimsPrincipal for AuthenticationState
            var authState = new AuthenticationState(mockUser);
            var mockAuthState = new Mock<AuthenticationState>(mockUser);


            // Act
            var result = loginRequest.DuoRequested(mockAuthState.Object);

            // Assert
            Assert.Equal(LoginResultStatus.DuoRequested, loginRequest.AuthenticationResult);
            Assert.Same(mockAuthState.Object, loginRequest.AuthenticationState);
            Assert.Same(loginRequest, result);
        }

        [Fact]
        public void GoogleAuthenticatorRegistrationRequested_ShouldSetResultAndStateAndReturnInstance()
        {
            // Arrange
            var loginRequest = new LoginRequest();
            var mockUser = new ClaimsPrincipal(new ClaimsIdentity());
            var mockAuthState = new Mock<AuthenticationState>(mockUser);

            // Act
            var result = loginRequest.GoogleAuthenticatorRegistrationRequested(mockAuthState.Object);

            // Assert
            Assert.Equal(LoginResultStatus.GoogleAuthenticatorRegistrationRequested, loginRequest.AuthenticationResult);
            Assert.Same(mockAuthState.Object, loginRequest.AuthenticationState);
            Assert.Same(loginRequest, result);
        }

        [Fact]
        public void GoogleAuthenticatorRequested_ShouldSetResultAndStateAndReturnInstance()
        {
            // Arrange
            var loginRequest = new LoginRequest();
            var mockUser = new ClaimsPrincipal(new ClaimsIdentity());
            var mockAuthState = new Mock<AuthenticationState>(mockUser);

            // Act
            var result = loginRequest.GoogleAuthenticatorRequested(mockAuthState.Object);

            // Assert
            Assert.Equal(LoginResultStatus.GoogleAuthenticatorRequested, loginRequest.AuthenticationResult);
            Assert.Same(mockAuthState.Object, loginRequest.AuthenticationState);
            Assert.Same(loginRequest, result);
        }

        [Fact]
        public void BadCredentials_ShouldSetResultAndReturnInstance()
        {
            // Arrange
            var loginRequest = new LoginRequest();

            // Act
            var result = loginRequest.BadCredentials();

            // Assert
            Assert.Equal(LoginResultStatus.BadCredentials, loginRequest.AuthenticationResult);
            Assert.Same(loginRequest, result);
        }

        [Fact]
        public void NoData_ShouldSetResultAndReturnInstance()
        {
            // Arrange
            var loginRequest = new LoginRequest();

            // Act
            var result = loginRequest.NoData();

            // Assert
            Assert.Equal(LoginResultStatus.NoData, loginRequest.AuthenticationResult);
            Assert.Same(loginRequest, result);
        }

        [Fact]
        public void NoUsername_ShouldSetResultAndReturnInstance()
        {
            // Arrange
            var loginRequest = new LoginRequest();

            // Act
            var result = loginRequest.NoUsername();

            // Assert
            Assert.Equal(LoginResultStatus.NoUsername, loginRequest.AuthenticationResult);
            Assert.Same(loginRequest, result);
        }

        [Fact]
        public void NoPassword_ShouldSetResultAndReturnInstance()
        {
            // Arrange
            var loginRequest = new LoginRequest();

            // Act
            var result = loginRequest.NoPassword();

            // Assert
            Assert.Equal(LoginResultStatus.NoPassword, loginRequest.AuthenticationResult);
            Assert.Same(loginRequest, result);
        }

        [Fact]
        public void UnknownFailure_ShouldSetResultAndReturnInstance()
        {
            // Arrange
            var loginRequest = new LoginRequest();

            // Act
            var result = loginRequest.UnknownFailure();

            // Assert
            Assert.Equal(LoginResultStatus.UnknownFailure, loginRequest.AuthenticationResult);
            Assert.Same(loginRequest, result);
        }

        [Fact]
        public void Success_ShouldSetResultAndStateAndReturnInstance()
        {
            // Arrange
            var loginRequest = new LoginRequest();
            var mockUser = new ClaimsPrincipal(new ClaimsIdentity());
            var mockAuthState = new Mock<AuthenticationState>(mockUser);


            // Act
            var result = loginRequest.Success(mockAuthState.Object);

            // Assert
            Assert.Equal(LoginResultStatus.OK, loginRequest.AuthenticationResult);
            Assert.Same(mockAuthState.Object, loginRequest.AuthenticationState);
            Assert.Same(loginRequest, result);
        }

        [Fact]
        public void DeniedLogin_ShouldSetResultAndReturnInstance()
        {
            // Arrange
            var loginRequest = new LoginRequest();

            // Act
            var result = loginRequest.DeniedLogin();

            // Assert
            Assert.Equal(LoginResultStatus.DeniedLogin, loginRequest.AuthenticationResult);
            Assert.Same(loginRequest, result);
        }
    }
}
