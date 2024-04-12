using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security;
using System.Security.Claims;
using System.Text.Json.Serialization;
using BLAZAM.Helpers;
using Microsoft.AspNetCore.Components.Authorization;

namespace BLAZAM.Common.Data
{
    public class LoginRequest
    {
        public Guid Id { get; set; }= Guid.NewGuid();
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
        public string? Password { get => password.ToPlainText(); set => password = value?.ToSecureString(); }
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
                    if (Password != null && Password.Length > 0)
                        return true;
                return false;
            }
        }

        /// <summary>
        /// Returns true if the login attempt is an impersonation request, hence no password is actually checked
        /// </summary>
        public bool Impersonation { get; set; } = false;
        /// <summary>
        /// To prevent a security hole, the impersonator identity is passed as a check to ensure this is not a XSS attack or an user without the appropriate privileges.
        /// </summary>
        public ClaimsPrincipal? ImpersonatorClaims { get; set; }

        /// <summary>
        /// The remote IP of the login attempt
        /// </summary>
        public string? IPAddress { get; set; }

        public string? MFAToken { get; set; }
        public string? MFARedirect { get; set; }

        public LoginResultStatus? AuthenticationResult { get; set; } = null;


        public LoginRequest UnauthorizedImpersonation()
        {
            AuthenticationResult = LoginResultStatus.UnauthorizedImpersonation;

            return this;
        }
        public LoginRequest MFARequested(AuthenticationState state)
        {
            AuthenticationState = state;
            AuthenticationResult = LoginResultStatus.MFARequested;

            return this;
        }

        public LoginRequest BadCredentials()
        {
            AuthenticationResult = LoginResultStatus.BadCredentials;

            return this;
        }
        public LoginRequest NoData()
        {
            AuthenticationResult = LoginResultStatus.NoData;

            return this;
        }
        public LoginRequest NoUsername()
        {
            AuthenticationResult = LoginResultStatus.NoUsername;

            return this;
        }
        public LoginRequest NoPassword()
        {
            AuthenticationResult = LoginResultStatus.NoPassword;

            return this;
        }



        public LoginRequest UnknownFailure()
        {
            AuthenticationResult = LoginResultStatus.UnknownFailure;

            return this;
        }
        [JsonIgnore]
        public AuthenticationState AuthenticationState { get; set; }
        public LoginRequest Success(AuthenticationState result)
        {
            AuthenticationResult = LoginResultStatus.OK;
            AuthenticationState = result;
            return this;
        }

        public LoginRequest DeniedLogin()
        {
            AuthenticationResult = LoginResultStatus.DeniedLogin;

            return this;
        }

    }
}
