using BLAZAM.Common.Data;
using BLAZAM.Server.Background;
using BLAZAM.Server.Data;
using BLAZAM.Server.Data.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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

        public bool _authenticating { get; set; }
        public bool _directoryAvailable { get; set; }
        public string _username { get; set; }
        public string _password { get; set; }
        public string RedirectUri { get; private set; }
        public AppAuthenticationStateProvider Auth { get; }
        public NavigationManager Nav { get; private set; }
        public ConnMonitor Monitor { get; private set; }
        public AuditLogger AuditLogger { get; private set; }

        public void OnGet(string returnUrl="")
        {
            ViewData["Layout"] = "_Layout";
            if (IsUrlLocalToHost(returnUrl))
            {
                RedirectUri = returnUrl;
            }
        }

        public static bool IsUrlLocalToHost(string url)
        {
            return ((url[0] == '/' && (url.Length == 1 ||
                    (url[1] != '/' && url[1] != '\\'))) ||   // "/" or "/foo" but not "//" or "/\"
                    (url.Length > 1 &&
                     url[0] == '~' && url[1] == '/'));   // "~/" or "~/foo"
        }

        [HttpPost]
        public async Task<IActionResult> OnPost([FromFormAttribute]LoginRequest req)
        {

            var result = await Auth.Login(req);
            if (result != null)
            {
                await HttpContext.SignInAsync(result.User);
                AuditLogger.Logon.Login(result.User);
                //return Redirect(req.ReturnUrl);
            }
            Response.Headers.Add("Refresh", "0.5");
            //Nav.NavigateTo(req.ReturnUrl, true);
            //return (IActionResult)Results.Ok();
                return Redirect(req.ReturnUrl);

        }


    }
}
