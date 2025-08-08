using BLAZAM.Services;
using BLAZAM.Services.Audit;
using BLAZAM.Services.Duo;
using BLAZAM.Session;
using BLAZAM.Session.Interfaces;
using DuoUniversal;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace BLAZAM.Pages
{
    public class MFACallbackModel : PageModel
    {
        private readonly WebUserAuditLogger _audit;
        private readonly AppAuthenticationStateProvider _auth;
        private readonly IDuoClientProvider _duoClientProvider;
        private readonly IApplicationUserStateService _userStateService;

        public MFACallbackModel(IDuoClientProvider duoClientProvider,
            IApplicationUserStateService userStateService,
            AppAuthenticationStateProvider appAuthenticationStateProvider,
            WebUserAuditLogger logger
            )
        {
            _audit = logger;
            _auth = appAuthenticationStateProvider;
            _duoClientProvider = duoClientProvider;
            _userStateService = userStateService;
        }

        public string AuthResponse { get; private set; }

        public async Task<IActionResult> OnGet(string? state = null, string? code = null)
        {
            // Duo should have sent a 'state' and 'code' parameter.  If either is missing or blank, something is wrong.
            if (string.IsNullOrWhiteSpace(state))
            {
                throw new DuoException("Required state value was empty");
            }
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new DuoException("Required code value was empty");
            }
            if (User != null && User.HasClaim(c => c.Type == ClaimTypes.Rsa))
            {
                if (state == User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Rsa)?.Value)
                {
                    //This is a valid callback for this user
                    var mFARequest = _userStateService.GetMFARequest(state);
                    if (mFARequest != null)
                    {
                        return await ProcessCallback(code, mFARequest);
                    }
                }
                else
                {
                    throw new DuoException("Session state did not match the expected state");
                }
            }
            return Page();
        }

        private async Task<IActionResult> ProcessCallback(string? code, MFARequest? mFARequest)
        {
            // Get the Duo client again.  This can be either be cached in the session or newly built.
            // The only stateful information in the Client is your configuration, so you could even use the same client for multiple
            // user authentications if desired.
            Client duoClient = _duoClientProvider.GetDuoClient(Request.Scheme + "://" + Request.Host + "/mfacallback");
            var username = mFARequest.user.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.WindowsAccountName)?.Value;
            // Get a summary of the authentication from Duo.  This will trigger an exception if the username does not match.
            try
            {
                IdToken token = await duoClient.ExchangeAuthorizationCodeFor2faResult(code, username);
                if (token.AuthResult.Result.Equals("allow", StringComparison.InvariantCultureIgnoreCase))
                {
                    var authenticatedState = await _auth.SetUser(mFARequest.user.User);
                    await HttpContext.SignInAsync(mFARequest.user.User);
                    await _audit.Logon.Login(mFARequest.user.User, HttpContext.Connection.RemoteIpAddress?.ToString());
                    return new RedirectResult(mFARequest.redirectUrl);
                }
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Warning(ex, "Error attempting to perform Duo MFA");
            }
            return new RedirectResult("/");

        }
    }
}
