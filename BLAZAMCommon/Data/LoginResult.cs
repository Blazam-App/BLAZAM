using Microsoft.AspNetCore.Components.Authorization;

namespace BLAZAM.Common.Data
{
    /// <summary>
    /// Represents the various possible outcomes of a login attempt.
    /// </summary>
    public enum LoginResultStatus
    {
        /// <summary>
        /// Login was successful.
        /// </summary>
        OK,
        
        /// <summary>
        /// Invalid username or password was provided.
        /// </summary>
        BadCredentials,
        
        /// <summary>
        /// User attempted to impersonate another user without proper authorization.
        /// </summary>
        UnauthorizedImpersonation,
        
        /// <summary>
        /// No data was received in the login request.
        /// </summary>
        NoData,
        
        /// <summary>
        /// Username was not provided in the login request.
        /// </summary>
        NoUsername,
        
        /// <summary>
        /// Password was not provided in the login request.
        /// </summary>
        NoPassword,
        
        /// <summary>
        /// Login failed for an unknown or unexpected reason.
        /// </summary>
        UnknownFailure,
        
        /// <summary>
        /// User's login attempt was denied due to insufficient permissions.
        /// </summary>
        DeniedLogin,
        
        /// <summary>
        /// Duo two-factor authentication is required to complete the login.
        /// </summary>
        DuoRequested,
        
        /// <summary>
        /// Google Authenticator verification is required to complete the login.
        /// </summary>
        GoogleAuthenticatorRequested,
        
        /// <summary>
        /// User needs to register their Google Authenticator device.
        /// </summary>
        GoogleAuthenticatorRegistrationRequested,
        
        /// <summary>
        /// User account is locked out due to too many failed login attempts.
        /// </summary>
        LockedOut
    }
    
   
}