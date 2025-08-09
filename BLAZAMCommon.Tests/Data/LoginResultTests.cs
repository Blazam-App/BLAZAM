using System.Security.Claims;
using BLAZAM.Common.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Moq;

namespace BLAZAMCommon.Tests.Data
{
    public class LoginResultTests
    {
        private AuthenticationState CreateTestAuthenticationState()
        {
            // Create a dummy ClaimsPrincipal for AuthenticationState
            // Moq is used here as per requirement for any mocking.
            var mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
            return new AuthenticationState(mockClaimsPrincipal.Object);
        }

        [Fact]
        public void Constructor_InitializesStatusToOKAndNullAuthenticationState()
        {
            // Arrange & Act
            var loginResult = new LoginResult();

            // Assert
            Assert.Equal(LoginResultStatus.OK, loginResult.Status); // Default enum value is 0 (OK)
            Assert.Null(loginResult.AuthenticationState);
        }

        [Fact]
        public void UnauthorizedImpersonation_SetsStatusCorrectly()
        {
            // Arrange
            var loginResult = new LoginResult();

            // Act
            var result = loginResult.UnauthorizedImpersonation();

            // Assert
            Assert.Equal(LoginResultStatus.UnauthorizedImpersonation, loginResult.Status);
            Assert.Null(loginResult.AuthenticationState); // Ensure AuthenticationState is not unintentionally set
            Assert.Same(loginResult, result); // Verify fluent return
        }

        [Fact]
        public void DuoRequested_SetsStatusAndAuthenticationStateCorrectly()
        {
            // Arrange
            var loginResult = new LoginResult();
            var expectedAuthState = CreateTestAuthenticationState();

            // Act
            var result = loginResult.DuoRequested(expectedAuthState);

            // Assert
            Assert.Equal(LoginResultStatus.DuoRequested, loginResult.Status);
            Assert.Same(expectedAuthState, loginResult.AuthenticationState);
            Assert.Same(loginResult, result);
        }

        [Fact]
        public void GoogleAuthenticatorRequested_SetsStatusAndAuthenticationStateCorrectly()
        {
            // Arrange
            var loginResult = new LoginResult();
            var expectedAuthState = CreateTestAuthenticationState();

            // Act
            var result = loginResult.GoogleAuthenticatorRequested(expectedAuthState);

            // Assert
            Assert.Equal(LoginResultStatus.GoogleAuthenticatorRequested, loginResult.Status);
            Assert.Same(expectedAuthState, loginResult.AuthenticationState);
            Assert.Same(loginResult, result);
        }

        [Fact]
        public void BadCredentials_SetsStatusCorrectly()
        {
            // Arrange
            var loginResult = new LoginResult();

            // Act
            var result = loginResult.BadCredentials();

            // Assert
            Assert.Equal(LoginResultStatus.BadCredentials, loginResult.Status);
            Assert.Null(loginResult.AuthenticationState);
            Assert.Same(loginResult, result);
        }

        [Fact]
        public void NoData_SetsStatusCorrectly()
        {
            // Arrange
            var loginResult = new LoginResult();

            // Act
            var result = loginResult.NoData();

            // Assert
            Assert.Equal(LoginResultStatus.NoData, loginResult.Status);
            Assert.Null(loginResult.AuthenticationState);
            Assert.Same(loginResult, result);
        }

        [Fact]
        public void NoUsername_SetsStatusCorrectly()
        {
            // Arrange
            var loginResult = new LoginResult();

            // Act
            var result = loginResult.NoUsername();

            // Assert
            Assert.Equal(LoginResultStatus.NoUsername, loginResult.Status);
            Assert.Null(loginResult.AuthenticationState);
            Assert.Same(loginResult, result);
        }

        [Fact]
        public void NoPassword_SetsStatusCorrectly()
        {
            // Arrange
            var loginResult = new LoginResult();

            // Act
            var result = loginResult.NoPassword();

            // Assert
            Assert.Equal(LoginResultStatus.NoPassword, loginResult.Status);
            Assert.Null(loginResult.AuthenticationState);
            Assert.Same(loginResult, result);
        }

        [Fact]
        public void UnknownFailure_SetsStatusCorrectly()
        {
            // Arrange
            var loginResult = new LoginResult();

            // Act
            var result = loginResult.UnknownFailure();

            // Assert
            Assert.Equal(LoginResultStatus.UnknownFailure, loginResult.Status);
            Assert.Null(loginResult.AuthenticationState);
            Assert.Same(loginResult, result);
        }

        [Fact]
        public void Success_SetsStatusAndAuthenticationStateCorrectly()
        {
            // Arrange
            var loginResult = new LoginResult();
            var expectedAuthState = CreateTestAuthenticationState();

            // Act
            var result = loginResult.Success(expectedAuthState);

            // Assert
            Assert.Equal(LoginResultStatus.OK, loginResult.Status);
            Assert.Same(expectedAuthState, loginResult.AuthenticationState);
            Assert.Same(loginResult, result);
        }

        [Fact]
        public void DeniedLogin_SetsStatusCorrectly()
        {
            // Arrange
            var loginResult = new LoginResult();

            // Act
            var result = loginResult.DeniedLogin();

            // Assert
            Assert.Equal(LoginResultStatus.DeniedLogin, loginResult.Status);
            Assert.Null(loginResult.AuthenticationState);
            Assert.Same(loginResult, result);
        }

        [Fact]
        public void ChainingMethods_LastCallDeterminesStatus()
        {
            // Arrange
            var loginResult = new LoginResult();
            var authState = CreateTestAuthenticationState();

            // Act
            loginResult.Success(authState) // Status: OK, AuthState: set
                       .BadCredentials()   // Status: BadCredentials, AuthState: still set
                       .DeniedLogin();     // Status: DeniedLogin, AuthState: still set

            // Assert
            Assert.Equal(LoginResultStatus.DeniedLogin, loginResult.Status);
            Assert.Same(authState, loginResult.AuthenticationState); // Check if AuthenticationState persists if not overwritten
        }

        [Fact]
        public void ChainingMethods_OverwritesAuthenticationState()
        {
            // Arrange
            var loginResult = new LoginResult();
            var firstAuthState = CreateTestAuthenticationState();
            var secondAuthState = CreateTestAuthenticationState();

            // Act
            loginResult.DuoRequested(firstAuthState)      // Status: DuoRequested, AuthState: firstAuthState
                       .Success(secondAuthState);         // Status: OK, AuthState: secondAuthState

            // Assert
            Assert.Equal(LoginResultStatus.OK, loginResult.Status);
            Assert.Same(secondAuthState, loginResult.AuthenticationState);
        }
    }
}
