using BLAZAM.Common.Data;
using BLAZAM.Services;
using BLAZAM.Services.Audit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BLAZAM.Server.Pages
{
    /// <summary>
    /// Represents the Single Sign-On (SSO) page model for handling authentication requests and responses.
    /// </summary>
    /// <remarks>This class provides methods for handling GET and POST requests related to user
    /// authentication. It integrates with the application's authentication state provider, navigation manager,
    /// connection monitor, and user audit logger to facilitate secure and auditable login operations.</remarks>
    [IgnoreAntiforgeryToken]
    public class SSOModel : PageModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SSOModel"/> class with the specified dependencies.
        /// </summary>
        /// <param name="auth">The authentication state provider used to manage user authentication state.</param>
        /// <param name="logger">The audit logger used to log user activity for auditing purposes.</param>
        public SSOModel(AppAuthenticationStateProvider auth, WebUserAuditLogger logger)
        {
            _auth = auth;
            _auditLogger = logger;
        }


        private readonly AppAuthenticationStateProvider _auth;
        private readonly WebUserAuditLogger _auditLogger;





        /// <summary>
        /// The authentication endpoint for web clients
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<IActionResult> OnPost([FromFormAttribute] LoginRequest req)
        {
            try
            {
                var result = await _auth.Login(req);
                if (result != null
                    && result.AuthenticationResult == LoginResultStatus.OK
                    && result.AuthenticationState != null)
                {
                    await HttpContext.SignInAsync(result.AuthenticationState.User);
                    await _auditLogger.Logon.Login(result.AuthenticationState.User);
                }

            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex, "SSOModel.OnPost: Exception during login attempt.");
            }
            if (req.ReturnUrl != null && req.ReturnUrl.IsUrlLocalToHost())
            {
                return Redirect(req.ReturnUrl);
            }
            return Redirect("/");

        }


    }
}
