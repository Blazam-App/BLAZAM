using BLAZAM.Services;
using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BLAZAM.Server.Pages
{
    /// <summary>
    /// Represents the page model for unimpersonating a user and reverting to the original user context.
    /// </summary>
    public class UninpersonateModel : PageModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UninpersonateModel"/> class.
        /// </summary>
        /// <param name="auth">The authentication state provider used to manage the application's authentication state.</param>
        /// <param name="uState">The service for managing the state of the currently authenticated user.</param>
        public UninpersonateModel(AppAuthenticationStateProvider auth, IApplicationUserStateService uState)
        {
            _auth = auth;
            _userState = uState;
        }

        private readonly AppAuthenticationStateProvider _auth;
        private readonly IApplicationUserStateService _userState;

        /// <summary>
        /// Handles the HTTP GET request to revert the current user to their original user state.
        /// </summary>
        /// <remarks>This method restores the original user principal if the current user is impersonating
        /// another user. Upon successful restoration, the current user state is removed, and the user is
        /// re-authenticated. The method then redirects the user to the permissions page.</remarks>
        /// <returns>A <see cref="Task{IActionResult}"/> representing the asynchronous operation.  The result is a redirection to
        /// the permissions page.</returns>
        public async Task<IActionResult> OnGet()
        {
            var currentState = _userState.CurrentUserState;
            var originalUserPrincipal = currentState.Impersonator;

            var result = await _auth.SetUser(originalUserPrincipal);
            if (result != null)
            {
                _userState.RemoveUserState(currentState);
                await HttpContext.SignInAsync(result.User);
            }
            return Redirect("/permissions");

        }

    }
}
