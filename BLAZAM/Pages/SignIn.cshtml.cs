using BLAZAM.Common.Data;
using BLAZAM.Services;
using BLAZAM.Services.Audit;
using BLAZAM.Services.Exceptions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BLAZAM.Server.Pages
{
    /// <summary>
    /// Handles user sign-in operations for web clients, including authentication, impersonation, and audit logging.
    /// Integrates with <see cref="AppAuthenticationStateProvider"/> for authentication and <see cref="WebUserAuditLogger"/> for logging user actions.
    /// </summary>
    [IgnoreAntiforgeryToken]
    public class SignInModel : PageModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SignInModel"/> class with authentication and audit logging services.
        /// </summary>
        /// <param name="auth">The authentication state provider.</param>
        /// <param name="logger">The audit logger for user actions.</param>
        public SignInModel(AppAuthenticationStateProvider auth, WebUserAuditLogger logger)
        {
            _auth = auth;
            _auditLogger = logger;
        }


        private readonly AppAuthenticationStateProvider _auth;
        private readonly WebUserAuditLogger _auditLogger;




        /// <summary>
        /// Handles POST requests for user authentication, including login, impersonation, and audit logging.
        /// Returns a JSON result with the login request outcome.
        /// </summary>
        /// <param name="req">The login request containing user credentials and context.</param>
        /// <returns>A <see cref="JsonResult"/> with the login outcome, or an <see cref="ObjectResult"/> with error details.</returns>
        public async Task<IActionResult> OnPost([FromFormAttribute] LoginRequest req)
        {
            try
            {
                req.IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                if (User != null)
                {
                    req.ImpersonatorClaims = User;
                }
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex, "Error setting ip address for login request");
            }
            try
            {

                var result = await _auth.Login(req);
                req.Password = null;
                req.AuthenticationResult = result.AuthenticationResult;
                if (result != null && (result.AuthenticationResult == LoginResultStatus.OK || result.AuthenticationResult == LoginResultStatus.DuoRequested))
                {

                    await HttpContext.SignInAsync(result.AuthenticationState.User);
                    if (result.AuthenticationState.User.Identity?.IsAuthenticated == true)
                        if (result.Impersonation)
                        {


                            await _auditLogger.Logon.Impersonate(User, result.AuthenticationState.User, req.IPAddress);

                        }
                        else
                        {
                            await _auditLogger.Logon.Login(result.AuthenticationState.User, req.IPAddress);

                        }
                    req.AuthenticationState = null;
                    req.ImpersonatorClaims = null;
                }
                return new JsonResult(req);

            }
            catch(GoogleMFARequestedException ex)
            {
                return new JsonResult(ex.LoginRequest);
            }
            catch (Exception ex)
            {
                return new ObjectResult(ex.Message);
            }


        }


    }
}
