using BLAZAM.Common.Data.Services;
using BLAZAM.Common.Extensions;
using BLAZAM.Server.Data.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace BLAZAM.Server.Pages.API.Auth
{
    //[Route("/api/auth/leepAlive")]
    public class KeepAliveModel : PageModel
    {
        public IActionResult OnGet()
        {

            var test = HttpContext.SessionTimeout();
                if (HttpContext.User.Identity.IsAuthenticated)
                    HttpContext.SlideCookieExpiration(ApplicationUserStateService.Instance.GetUserState(HttpContext.User));
                else
                {
                    var redirectResponse = new Dictionary<string, string>() { { "expired", "true" } };
                    return new JsonResult(redirectResponse);
                }
            
            return new OkResult();
        
        }

    }
}
