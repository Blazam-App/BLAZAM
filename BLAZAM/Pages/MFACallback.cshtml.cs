using System.Security.Claims;
using BLAZAM.Services;
using BLAZAM.Services.Audit;
using BLAZAM.Services.Duo;
using BLAZAM.Session;
using BLAZAM.Session.Interfaces;
using DuoUniversal;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BLAZAM.Pages
{
    /// <summary>
    /// Handles the callback from Duo Multi-Factor Authentication (MFA) during the authentication process.
    /// This class processes the response from Duo, validates the state and code parameters,
    /// and completes the authentication flow for the user.
    /// </summary>
    public class MFACallbackModel : PageModel
    {
        private readonly WebUserAuditLogger _audit;
        private readonly AppAuthenticationStateProvider _auth;
        private readonly IDuoClientProvider _duoClientProvider;
        private readonly IApplicationUserStateService _userStateService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MFACallbackModel"/> class with required services for MFA processing.
        /// </summary>
        /// <param name="duoClientProvider">Provides Duo client instances for MFA operations.</param>
        /// <param name="userStateService">Manages application user state and MFA requests.</param>
        /// <param name="appAuthenticationStateProvider">Handles authentication state transitions.</param>
        /// <param name="logger">Logs user authentication and audit events.</param>
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

        /// <summary>
        /// Gets the authentication response message from the Duo callback.
        /// </summary>
        public string AuthResponse { get; private set; }

        /// <summary>
        /// Handles the GET request from Duo's MFA callback, validates the state and code,
        /// and processes the authentication result.
        /// </summary>
        /// <param name="state">The state parameter returned by Duo, used to correlate the MFA request.</param>
        /// <param name="code">The authorization code returned by Duo for MFA verification.</param>
        /// <returns>An <see cref="IActionResult"/> representing the outcome of the authentication process.</returns>
        /// <exception cref="DuoException">Thrown if required parameters are missing or invalid.</exception>
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

        /// <summary>
        /// Processes the Duo callback by exchanging the authorization code for a 2FA result,
        /// authenticating the user, and redirecting based on the outcome.
        /// </summary>
        /// <param name="code">The authorization code from Duo.</param>
        /// <param name="mFARequest">The MFA request containing user and redirect information.</param>
        /// <returns>An <see cref="IActionResult"/> indicating success or failure of authentication.</returns>
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
