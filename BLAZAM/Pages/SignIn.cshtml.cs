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
    public class SignInModel : PageModel
    {
        public SignInModel(AppAuthenticationStateProvider auth, NavigationManager _nav, ConnMonitor _monitor, AuditLogger logger)
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

        public void OnGet(string returnUrl = "")
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
        public async Task<IActionResult> OnPost([FromFormAttribute] LoginRequest req)
        {
            try
            {
                req.IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error("Error setting ip address for login request {@Error}", ex);
            }
            try
            {

                var result = await Auth.Login(req);
                req.Password = null;
                req.AuthenticationResult = result.AuthenticationResult;
                if (result != null && (result.AuthenticationResult == LoginResultStatus.OK || result.AuthenticationResult == LoginResultStatus.MFARequested))
                {

                    await HttpContext.SignInAsync(result.AuthenticationState.User);
                    if (result.AuthenticationState.User.Identity?.IsAuthenticated == true)
                        await AuditLogger.Logon.Login(result.AuthenticationState.User, req.IPAddress);
                }
                return new JsonResult(req);

            }
            catch (Exception ex)
            {
                return new ObjectResult(ex.Message);
            }


        }


    }
}
