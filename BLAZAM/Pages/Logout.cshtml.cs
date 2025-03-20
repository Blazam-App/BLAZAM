using BLAZAM.Services;
using BLAZAM.Services.Audit;
using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BLAZAM.Server.Pages
{
    public class LogOutModel : PageModel
    {
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
