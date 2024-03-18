using BLAZAM.Common.Data;
using BLAZAM.Services;
using BLAZAM.Server.Data.Services;
using BLAZAM.Services.Background;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BLAZAM.Helpers;
using BLAZAM.Gui.UI.Dashboard.Widgets;
using BLAZAM.Services.Audit;

namespace BLAZAM.Server.Pages
{
    [IgnoreAntiforgeryToken]
    public class SSOModel : PageModel
    {
        public SSOModel(AppAuthenticationStateProvider auth, NavigationManager _nav,ConnMonitor _monitor,AuditLogger logger)
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

        public IActionResult OnGet(string returnUrl="")
        {
            ViewData["Layout"] = "_Layout";
            if (returnUrl.IsUrlLocalToHost())
            {
                RedirectUri = returnUrl;
            }
            return Redirect("/");

        }


        /// <summary>
        /// The authentication endpoint for web clients
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<IActionResult> OnPost([FromFormAttribute]LoginRequest req)
        {
            try
            {
                var result = await Auth.Login(req);
                if (result != null && result.Status == LoginResultStatus.OK)
                {
                    await HttpContext.SignInAsync(result.AuthenticationState.User);
                    await AuditLogger.Logon.Login(result.AuthenticationState.User);
                }
               // return new ObjectResult(result.Status);

            }
            catch 
            {

                //return new ObjectResult(ex.Message);
            }
            if (req.ReturnUrl!=null && req.ReturnUrl.IsUrlLocalToHost())
            {
                return Redirect(req.ReturnUrl);
            }
            return Redirect("/");

        }


    }
}
