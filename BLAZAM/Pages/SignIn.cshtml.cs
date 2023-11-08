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
using DuoUniversal;
using Octokit;
using BLAZAM.Services.Duo;

namespace BLAZAM.Server.Pages
{
    [IgnoreAntiforgeryToken]
    public class SignInModel : PageModel
    {
        public SignInModel(AppAuthenticationStateProvider auth, NavigationManager _nav,ConnMonitor _monitor,AuditLogger logger, IDuoClientProvider duoClientProvider)
        {
            Auth = auth;
            Nav = _nav;
            Monitor = _monitor;
            AuditLogger = logger;
            _duoClientProvider = duoClientProvider;
        }


        public string RedirectUri { get; set; }
        public AppAuthenticationStateProvider Auth { get; }
        public NavigationManager Nav { get; private set; }
        public ConnMonitor Monitor { get; private set; }
        public AuditLogger AuditLogger { get; private set; }

        private readonly IDuoClientProvider _duoClientProvider;
        internal const string STATE_SESSION_KEY = "_State";
        internal const string USERNAME_SESSION_KEY = "_Username";


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
            try
            {
                // Initiate the Duo authentication for a specific username

                // Get a Duo client
                Client duoClient = _duoClientProvider.GetDuoClient();

                // Check if Duo seems to be healthy and able to service authentications.
                // If Duo were unhealthy, you could possibly send user to an error page, or implement a fail mode
                var isDuoHealthy = await duoClient.DoHealthCheck();

                // Generate a random state value to tie the authentication steps together
                string state = Client.GenerateState();
                // Save the state and username in the session for later
                HttpContext.Session.SetString(STATE_SESSION_KEY, state);
                HttpContext.Session.SetString(USERNAME_SESSION_KEY, req.Username);

                // Get the URI of the Duo prompt from the client.  This includes an embedded authentication request.
                string promptUri = duoClient.GenerateAuthUri(req.Username, state);

                // Redirect the user's browser to the Duo prompt.
                // The Duo prompt, after authentication, will redirect back to the configured Redirect URI to complete the authentication flow.
                // In this example, that is /duo_callback, which is implemented in Callback.cshtml.cs.
                return new ObjectResult(promptUri);


                var result = await Auth.Login(req);
                if (result != null && result.Status == LoginResultStatus.OK)
                {
                    await HttpContext.SignInAsync(result.AuthenticationState.User);
                    await AuditLogger.Logon.Login(result.AuthenticationState.User);
                }
                return new ObjectResult(result.Status);

            }
            catch (Exception ex)
            {
                return new ObjectResult(ex.Message);
            }
           
            
        }


    }
}
