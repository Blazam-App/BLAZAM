using BLAZAM.Common.Data.Services;
using BLAZAM.Server.Data;
using BLAZAM.Server.Data.Services;
using BLAZAM.Services;
using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BLAZAM.Server.Pages
{
    public class UninpersonateModel : PageModel
    {
        public UninpersonateModel(AppAuthenticationStateProvider auth, NavigationManager _nav, IApplicationUserStateService uState)
        {
            Auth = auth;
            Nav = _nav;
            UserState = uState;
        }

        public bool _authenticating { get; set; }
        public bool _directoryAvailable { get; set; }
        public string _username { get; set; }
        public string _password { get; set; }
        public string RedirectUri { get; private set; }
        public AppAuthenticationStateProvider Auth { get; }
        public NavigationManager Nav { get; private set; }
        public IApplicationUserStateService UserState { get; private set; }

        public async Task<IActionResult> OnGet()
        {
            var currentState = UserState.CurrentUserState;
            var originalUserPrincipal = currentState.Impersonator;

            var result = await Auth.SetUser(originalUserPrincipal);
            if (result != null)
            {
                UserState.RemoveUserState(currentState);
                await HttpContext.SignInAsync(result.User);
            }
            return Redirect("/permissions");

        }

    }
}
