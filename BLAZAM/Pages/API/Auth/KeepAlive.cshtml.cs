
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

            var response = new Dictionary<string, string>();
                if (HttpContext.User.Identity?.IsAuthenticated==true)
                    HttpContext.SlideCookieExpiration(ApplicationUserStateService.Instance.GetUserState(HttpContext.User));
                else
                {
                     response.Add("expired", "true");
                    return new JsonResult(response);
                }
                     response.Add("expired", "false");
            return new JsonResult(response);

            //return new OkResult();
        
        }

    }
}
