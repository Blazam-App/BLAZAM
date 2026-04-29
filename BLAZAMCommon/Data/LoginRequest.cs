using BLAZAM.Helpers;
using Microsoft.AspNetCore.Components.Authorization;
using System.ComponentModel.DataAnnotations;
using System.Security;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace BLAZAM.Common.Data
{
    /// <summary>
    /// Represents a login request containing user credentials and authentication context for the BLAZAM application.
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Gets or sets the unique identifier for this login request.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();
        
        private SecureString? password;
        
        /// <summary>
        /// The username provided during the app login attempt
        /// </summary>
        [Required]
        public string? Username { get; set; }
        
        /// <summary>
        /// The password provided during the app login attempt
        /// </summary>
        /// <remarks>
        /// This value is automatically stored as SecureString to prevent memory sniff exposure
        /// </remarks>
        [Required]
        public string? Password { get => password == null ? null : password.ToPlainText(); set => password = value?.ToSecureString(); }
        
        /// <summary>
        /// Gets the secure representation of the password.
        /// </summary>
        public SecureString? SecurePassword => password;
        
        /// <summary>
        /// The relative url to return to after login success
        /// </summary>
        public string? ReturnUrl { get; set; } = "/";

        /// <summary>
        /// The base address to callback MFA requests
        /// </summary>
        public string? CallbackBaseUri { get; set; }


        /// <summary>
        /// Returns true if the username and password are both present.
        /// </summary>
        public bool Valid
        {
            get
            {
                if (Username != null && Username.Length > 0)
                {
                    if (Password != null && Password.Length > 0)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Returns true if the login attempt is an impersonation request, hence no password is actually checked
        /// </summary>
        public bool Impersonation { get; set; } = false;
        
        /// <summary>
        /// To prevent a security hole, the impersonator identity is passed as a check to ensure this is not a XSS attack or a user without the appropriate privileges.
        /// </summary>
        public ClaimsPrincipal? ImpersonatorClaims { get; set; }

        /// <summary>
        /// The remote IP of the login attempt
        /// </summary>
        public string? IPAddress { get; set; }

        /// <summary>
        /// Gets or sets the multi-factor authentication token.
        /// </summary>
        public string? MFAToken { get; set; }
        
        /// <summary>
        /// Gets or sets the redirect URL for multi-factor authentication.
        /// </summary>
        public string? MFARedirect { get; set; }


        /// <summary>
        /// Gets or sets the result status of the authentication attempt.
        /// </summary>
        public LoginResultStatus? AuthenticationResult { get; set; } = null;


        /// <summary>
        /// Marks the login request as an unauthorized impersonation attempt.
        /// </summary>
        /// <returns>The current <see cref="LoginRequest"/> instance for method chaining.</returns>
        public LoginRequest UnauthorizedImpersonation()
        {
            AuthenticationResult = LoginResultStatus.UnauthorizedImpersonation;

            return this;
        }
        
        /// <summary>
        /// Marks the login request as requiring Duo multi-factor authentication.
        /// </summary>
        /// <param name="state">The authentication state to preserve.</param>
        /// <param name="mfaRedirect">The URL to redirect for MFA verification.</param>
        /// <returns>The current <see cref="LoginRequest"/> instance for method chaining.</returns>
        public LoginRequest DuoRequested(AuthenticationState state, string? mfaRedirect)
        {
            MFARedirect = mfaRedirect;
            AuthenticationState = state;
            AuthenticationResult = LoginResultStatus.DuoRequested;

            return this;
        }
        
        /// <summary>
        /// Marks the login request as requiring Google Authenticator registration.
        /// </summary>
        /// <param name="state">The authentication state to preserve.</param>
        /// <returns>The current <see cref="LoginRequest"/> instance for method chaining.</returns>
        public LoginRequest GoogleAuthenticatorRegistrationRequested(AuthenticationState state)
        {
            AuthenticationState = state;
            AuthenticationResult = LoginResultStatus.GoogleAuthenticatorRegistrationRequested;

            return this;
        }
        
        /// <summary>
        /// Marks the login request as requiring Google Authenticator verification.
        /// </summary>
        /// <param name="state">The authentication state to preserve.</param>
        /// <returns>The current <see cref="LoginRequest"/> instance for method chaining.</returns>
        public LoginRequest GoogleAuthenticatorRequested(AuthenticationState state)

        {
            AuthenticationState = state;
            AuthenticationResult = LoginResultStatus.GoogleAuthenticatorRequested;

            return this;
        }

        /// <summary>
        /// Marks the login request as failed due to invalid credentials.
        /// </summary>
        /// <returns>The current <see cref="LoginRequest"/> instance for method chaining.</returns>
        public LoginRequest BadCredentials()
        {
            AuthenticationResult = LoginResultStatus.BadCredentials;

            return this;
        }
        
        /// <summary>
        /// Marks the login request as failed due to missing data.
        /// </summary>
        /// <returns>The current <see cref="LoginRequest"/> instance for method chaining.</returns>
        public LoginRequest NoData()
        {
            AuthenticationResult = LoginResultStatus.NoData;

            return this;
        }
        
        /// <summary>
        /// Marks the login request as failed due to missing username.
        /// </summary>
        /// <returns>The current <see cref="LoginRequest"/> instance for method chaining.</returns>
        public LoginRequest NoUsername()
        {
            AuthenticationResult = LoginResultStatus.NoUsername;

            return this;
        }
        
        /// <summary>
        /// Marks the login request as failed due to missing password.
        /// </summary>
        /// <returns>The current <see cref="LoginRequest"/> instance for method chaining.</returns>
        public LoginRequest NoPassword()
        {
            AuthenticationResult = LoginResultStatus.NoPassword;

            return this;
        }



        /// <summary>
        /// Marks the login request as failed due to an unknown error.
        /// </summary>
        /// <returns>The current <see cref="LoginRequest"/> instance for method chaining.</returns>
        public LoginRequest UnknownFailure()
        {
            AuthenticationResult = LoginResultStatus.UnknownFailure;

            return this;
        }
        
        /// <summary>
        /// Gets or sets the authentication state resulting from this login request.
        /// </summary>
        [JsonIgnore]
        public AuthenticationState? AuthenticationState { get; set; }

        /// <summary>
        /// Marks the login request as successful and sets the authentication state.
        /// </summary>
        /// <param name="result">The successful authentication state.</param>
        /// <returns>The current <see cref="LoginRequest"/> instance for method chaining.</returns>
        public LoginRequest Success(AuthenticationState result)
        {
            AuthenticationResult = LoginResultStatus.OK;
            AuthenticationState = result;
            return this;
        }

        /// <summary>
        /// Marks the login request as denied due to insufficient permissions or policy restrictions.
        /// </summary>
        /// <returns>The current <see cref="LoginRequest"/> instance for method chaining.</returns>
        public LoginRequest DeniedLogin()
        {
            AuthenticationResult = LoginResultStatus.DeniedLogin;

            return this;
        }

        /// <summary>
        /// Marks the login request as failed due to account lockout.
        /// </summary>
        /// <returns>The current <see cref="LoginRequest"/> instance for method chaining.</returns>
        public LoginRequest LockedOut()
        {
            AuthenticationResult = LoginResultStatus.LockedOut;

            return this;
        }
    }
}
