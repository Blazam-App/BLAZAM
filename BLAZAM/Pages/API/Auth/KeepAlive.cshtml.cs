using BLAZAM.Session;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BLAZAM.Pages.API.Auth
{
    /// <summary>
    /// An endpoint for the client to check in to, to extend authentication expiration
    /// </summary>
    public class KeepAliveModel : PageModel
    {
        /// <summary>
        /// Called when the page is accessed via GET request.
        /// </summary>
        /// <returns></returns>
        public IActionResult OnGet()
        {

            var response = new Dictionary<string, string>();
            if (HttpContext.User.Identity?.IsAuthenticated == true)
            {
                HttpContext.SlideCookieExpiration(ApplicationUserStateService.Instance.GetUserState(HttpContext.User));

            }
            else
            {
                response.Add("expired", "true");
                return new JsonResult(response);
            }

            response.Add("expired", "false");
            return new JsonResult(response);


        }

    }
}
