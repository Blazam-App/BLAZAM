using BLAZAM.Common.Data.Services;
using BLAZAM.Server.Data;
using BLAZAM.Server.Data.Services;
using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BLAZAM.Server.Pages
{
    public class LogOutModel : PageModel
    {
        public LogOutModel(AppAuthenticationStateProvider auth, NavigationManager _nav, AuditLogger logger, IApplicationUserStateService uss)
        {
            Auth = auth;
            Nav = _nav;
            AuditLogger = logger;
            UserStateService = uss;
        }

        public bool _authenticating { get; set; }
        public bool _directoryAvailable { get; set; }
        public string _username { get; set; }
        public string _password { get; set; }
        public string RedirectUri { get; private set; }
        public AppAuthenticationStateProvider Auth { get; }
        public NavigationManager Nav { get; private set; }
        public AuditLogger AuditLogger { get; private set; }
        public IApplicationUserStateService UserStateService { get; private set; }

        public async Task<IActionResult> OnGet()
        {
            var user = this.User;
            var state = UserStateService.GetUserState(user);
            if (state.User.Identity.IsAuthenticated)
            {
                await AuditLogger.Logon.Logout();

                var result = Auth.Logout(User);
                if (result != null)
                {
                    await HttpContext.SignOutAsync();

                }
            }
            return Redirect("/");

        }

    }
}
