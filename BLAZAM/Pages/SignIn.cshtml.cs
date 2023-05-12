using BLAZAM.Common.Data;
using BLAZAM.Services;
using BLAZAM.Server.Data.Services;
using BLAZAM.Services.Background;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BLAZAM.Helpers;

namespace BLAZAM.Server.Pages
{
    [IgnoreAntiforgeryToken]
    public class SignInModel : PageModel
    {
        public SignInModel(AppAuthenticationStateProvider auth, NavigationManager _nav,ConnMonitor _monitor,AuditLogger logger)
        {
            Auth = auth;
            Nav = _nav;
            Monitor = _monitor;
            AuditLogger = logger;
        }


        public string RedirectUri { get; set; }
        public AppAuthenticationStateProvider Auth { get; }
        public NavigationManager Nav { get; private set; }
        public ConnMonitor Monitor { get; private set; }
        public AuditLogger AuditLogger { get; private set; }

        public void OnGet(string returnUrl="")
        {
            ViewData["Layout"] = "_Layout";
            if (returnUrl.IsUrlLocalToHost())
            {
                RedirectUri = returnUrl;
            }
        }

      
        /// <summary>
        /// The authentication endpoint for web clients
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<IActionResult> OnPost([FromFormAttribute]LoginRequest req)
        {
          
                var result = await Auth.Login(req);
                if (result != null && result.Status == LoginResultStatus.OK)
                {
                    await HttpContext.SignInAsync(result.AuthenticationState.User);
                    await AuditLogger.Logon.Login(result.AuthenticationState.User);
                }
           
           
            
            return new ObjectResult(result.Status);
        }


    }
}
