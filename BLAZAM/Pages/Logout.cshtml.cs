using BLAZAM.Services;
using BLAZAM.Services.Audit;
using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BLAZAM.Server.Pages
{
    /// <summary>
    /// Represents the page model for logging out a user.
    /// </summary>
    /// <remarks>This class provides functionality to log out the currently authenticated user and redirect
    /// them to the home page. It relies on services for authentication state management, user state retrieval, and
    /// audit logging.</remarks>
    public class LogOutModel : PageModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogOutModel"/> class.
        /// </summary>
        /// <param name="auth">The authentication state provider used to manage user authentication state.</param>
        /// <param name="logger">The logger used to record user audit events.</param>
        /// <param name="uss">The service used to manage application user state.</param>
        public LogOutModel(AppAuthenticationStateProvider auth, WebUserAuditLogger logger, IApplicationUserStateService uss)
        {
            _auth = auth;
            _auditLogger = logger;
            _userStateService = uss;
        }

        private readonly AppAuthenticationStateProvider _auth;
        private readonly WebUserAuditLogger _auditLogger;
        private readonly IApplicationUserStateService _userStateService;

        /// <summary>
        /// Logs out the currently logged on user
        /// </summary>
        /// <returns>A redirect to "/"</returns>
        public async Task<IActionResult> OnGet()
        {
            var user = this.User;
            var state = _userStateService.GetUserState(user);
            if (state?.User.Identity?.IsAuthenticated == true)
            {
                await _auditLogger.Logon.Logout();

                var result = await _auth.Logout(User);
                if (result != null)
                {
                    await HttpContext.SignOutAsync();

                }
            }
            return Redirect("/");

        }

    }
}
