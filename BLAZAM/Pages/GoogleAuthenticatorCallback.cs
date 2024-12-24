using BLAZAM.Server;
using BLAZAM.Services;
using BLAZAM.Services.Audit;
using BLAZAM.Services.Duo;
using BLAZAM.Session.Interfaces;
using DuoUniversal;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Identity.Client;
using Octokit;
using Org.BouncyCastle.Ocsp;
using System.Security.Claims;
using System.Text.Json;

namespace BLAZAM.Pages
{
    public class GoogleAuthenticatorCallbackModel : PageModel
    {
        private readonly AuditLogger _audit;
        private readonly AppAuthenticationStateProvider _auth;
        private readonly GoogleAuthenticatorService _googleAuthenticator;
        private readonly IApplicationUserStateService _userStateService;

        public GoogleAuthenticatorCallbackModel(GoogleAuthenticatorService googleAuthenticator,
            IApplicationUserStateService userStateService,
            AppAuthenticationStateProvider appAuthenticationStateProvider,
            AuditLogger logger
            )
        {
            _audit = logger;
            _auth = appAuthenticationStateProvider;
            _googleAuthenticator = googleAuthenticator;
            _userStateService = userStateService;
        }

        public string AuthResponse { get; private set; }

        public async Task<IActionResult> OnGet(string? token = null, string? code = null)
        {
            // Duo should have sent a 'state' and 'code' parameter.  If either is missing or blank, something is wrong.
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new DuoException("Required state value was empty");
            }
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new DuoException("Required code value was empty");
            }
            if (User != null)
            {
               

                    //This is a valid callback for this user
                    var mFARequest = _userStateService.GetMFARequest(token);
                    if (mFARequest != null)
                    {



                        // Get the Duo client again.  This can be either be cached in the session or newly built.
                        // The only stateful information in the Client is your configuration, so you could even use the same client for multiple
                        // user authentications if desired.
                        // Get a summary of the authentication from Duo.  This will trigger an exception if the username does not match.
                        try
                        {
                           if (_googleAuthenticator.ValidateTwoFactorPIN(mFARequest.mfaToken, code))
                            {
                                var authenticatedState = await _auth.SetUser(mFARequest.user.User);
                                await HttpContext.SignInAsync(mFARequest.user.User);
                                await _audit.Logon.Login(mFARequest.user.User, HttpContext.Connection.RemoteIpAddress?.ToString());
                                return new RedirectResult(mFARequest.redirectUrl);
                            }
                        }
                        catch
                        {

                            return new RedirectResult("/");
                        }



                    }




               

            }
            return Page();
        }
    }
}
