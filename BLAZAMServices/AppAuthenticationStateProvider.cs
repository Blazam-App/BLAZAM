using System.Security.Claims;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data.Services;
using BLAZAM.Common.Exceptions;
using BLAZAM.Database.Context;
using BLAZAM.Database.Models;
using BLAZAM.Database.Models.User;
using BLAZAM.Helpers;
using BLAZAM.Logger;
using BLAZAM.Server.Helpers;
using BLAZAM.Services.Audit;
using BLAZAM.Services.Background;
using BLAZAM.Services.Duo;
using BLAZAM.Session;
using BLAZAM.Session.Interfaces;
using DuoUniversal;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;


namespace BLAZAM.Services
{
    /// <summary>
    /// Manages user authentication states, including login, logout, impersonation, and MFA flows, integrating with Active Directory and other authentication services.
    /// </summary>
    public class AppAuthenticationStateProvider : AuthenticationStateProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppAuthenticationStateProvider"/> class with necessary dependencies for authentication and user state management.
        /// </summary>
        /// <param name="factory">The user database factory.</param>
        /// <param name="directory">The Active Directory context.</param>
        /// <param name="permissionHandler">The permission applicator service.</param>
        /// <param name="userStateService">The application user state service.</param>
        /// <param name="ca">The HTTP context accessor.</param>
        /// <param name="dcp">The Duo client provider.</param>
        /// <param name="enc">The encryption service.</param>
        /// <param name="audit">The web user audit logger.</param>
        /// <param name="applicationInfo">The application information service.</param>
        /// <param name="googleAuthenticatorService">The Google Authenticator service.</param>
        /// <exception cref="ArgumentNullException">Thrown if any critical dependency is null.</exception>
        public AppAuthenticationStateProvider(IUserDatabaseFactory factory,
            IActiveDirectoryContext directory, // Corrected typo: directoy -> directory
            PermissionApplicator permissionHandler,
            IApplicationUserStateService userStateService,
            IHttpContextAccessor ca,
            IDuoClientProvider dcp,
            IEncryptionService enc,
            WebUserAuditLogger audit,
            ApplicationInfo applicationInfo,
            GoogleAuthenticatorService googleAuthenticatorService)
        {
            if (factory == null) { Loggers.SystemLogger.Error("Dependency {DependencyName} is null in AppAuthenticationStateProvider constructor.", nameof(factory)); throw new ArgumentNullException(nameof(factory)); }
            if (directory == null) { Loggers.SystemLogger.Error("Dependency {DependencyName} is null in AppAuthenticationStateProvider constructor.", nameof(directory)); throw new ArgumentNullException(nameof(directory)); }
            if (permissionHandler == null) { Loggers.SystemLogger.Error("Dependency {DependencyName} is null in AppAuthenticationStateProvider constructor.", nameof(permissionHandler)); throw new ArgumentNullException(nameof(permissionHandler)); }
            if (userStateService == null) { Loggers.SystemLogger.Error("Dependency {DependencyName} is null in AppAuthenticationStateProvider constructor.", nameof(userStateService)); throw new ArgumentNullException(nameof(userStateService)); }
            if (ca == null) { Loggers.SystemLogger.Error("Dependency {DependencyName} is null in AppAuthenticationStateProvider constructor.", nameof(ca)); throw new ArgumentNullException(nameof(ca)); }
            if (dcp == null) { Loggers.SystemLogger.Error("Dependency {DependencyName} is null in AppAuthenticationStateProvider constructor.", nameof(dcp)); throw new ArgumentNullException(nameof(dcp)); }
            if (enc == null) { Loggers.SystemLogger.Error("Dependency {DependencyName} is null in AppAuthenticationStateProvider constructor.", nameof(enc)); throw new ArgumentNullException(nameof(enc)); }
            if (audit == null) { Loggers.SystemLogger.Error("Dependency {DependencyName} is null in AppAuthenticationStateProvider constructor.", nameof(audit)); throw new ArgumentNullException(nameof(audit)); }
            if (applicationInfo == null) { Loggers.SystemLogger.Error("Dependency {DependencyName} is null in AppAuthenticationStateProvider constructor.", nameof(applicationInfo)); throw new ArgumentNullException(nameof(applicationInfo)); }
            if (googleAuthenticatorService == null) { Loggers.SystemLogger.Error("Dependency {DependencyName} is null in AppAuthenticationStateProvider constructor.", nameof(googleAuthenticatorService)); throw new ArgumentNullException(nameof(googleAuthenticatorService)); }

            _applicationInfo = applicationInfo;
            this._encryption = enc;
            this._googleAuthenticatorService = googleAuthenticatorService;
            this._directory = directory; // Corrected typo: directoy -> directory
            this._factory = factory;
            this._permissionHandler = permissionHandler;
            this._userStateService = userStateService;
            this._httpContextAccessor = ca;
            this.CurrentUser = GetAnonymous(ca.HttpContext?.Session.Id);

            this._duoClientProvider = dcp;
            this._audit = audit;
        }

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDuoClientProvider _duoClientProvider;
        private readonly WebUserAuditLogger _audit;
        private readonly ApplicationInfo _applicationInfo;
        private readonly IEncryptionService _encryption;
        private readonly GoogleAuthenticatorService _googleAuthenticatorService;
        private readonly IActiveDirectoryContext _directory; // Corrected typo: directoy -> directory
        private readonly IUserDatabaseFactory _factory;
        private readonly PermissionApplicator _permissionHandler;

        private readonly IApplicationUserStateService _userStateService;


        public static Action<CookieAuthenticationOptions> ApplyAuthenticationCookieOptions()
        {
            return options =>
            {

                options.Events.OnSigningIn = async (context) =>
                {
                    if (DatabaseCache.AuthenticationSettings?.SessionTimeout != null)
                    {
                        var currentUtc = DateTimeOffset.UtcNow;
                        context.Properties.IssuedUtc = currentUtc;
                        context.Properties.ExpiresUtc = currentUtc.AddMinutes((double)DatabaseCache.AuthenticationSettings.SessionTimeout);
                    }
                };

                options.Events.OnValidatePrincipal = async (context) =>
                {
                    if (DatabaseCache.AuthenticationSettings?.SessionTimeout != null)
                    {
                        var currentUtc = DateTimeOffset.UtcNow;
                        context.Properties.IssuedUtc = currentUtc;
                        context.Properties.ExpiresUtc = currentUtc.AddMinutes((double)DatabaseCache.AuthenticationSettings.SessionTimeout);
                    }
                };
                options.LoginPath = new PathString("/login");
                options.LogoutPath = new PathString("/logout");
                if (DatabaseCache.AuthenticationSettings?.SessionTimeout != null)
                    options.ExpireTimeSpan = TimeSpan.FromMinutes((double)DatabaseCache.AuthenticationSettings.SessionTimeout);
                else
                    options.ExpireTimeSpan = TimeSpan.FromSeconds(10);

                options.SlidingExpiration = true;
            };
        }

        private ClaimsPrincipal? CurrentUser;

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var task = Task.FromResult(new AuthenticationState(this.CurrentUser));

            return task;
        }

        /// <summary>
        /// Creates an anonymous ClaimsPrincipal to handle authentication
        /// before login.
        /// </summary>
        /// <returns>An unauthenticated anonymous User ClaimsPrincipal</returns>
        private static ClaimsPrincipal GetAnonymous(string? sessionId = null, string? mfaToken = null)
        {

            var identity = new ClaimsIdentity(new[]
           {
                    new Claim(ClaimTypes.Sid, sessionId.IsNullOrEmpty()?"0":sessionId),
                    new Claim(ClaimTypes.Name, "Anonymous"),
                    new Claim(ClaimTypes.Role, "Anonymous"),
                    new Claim(ClaimTypes.Actor,sessionId.IsNullOrEmpty()?"0":sessionId)
                }, null);
            if (mfaToken != null)
            {
                identity.AddClaim(new Claim(ClaimTypes.Rsa, mfaToken));
            }

            return new ClaimsPrincipal(identity);
        }
        private static ClaimsPrincipal GetDemoUser()
        {
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Sid, "2"),
                new Claim(ClaimTypes.Name, "Demo"),
                new Claim(ClaimTypes.Actor, "2")
            };
            claims.AddSuperAdmin();
            claims.AddAllRoles();
            var identity = new ClaimsIdentity(claims.ToArray(), AppAuthenticationTypes.LocalAuthentication);
            return new ClaimsPrincipal(identity);
        }
        private static ClaimsPrincipal GetLocalAdmin(string name = "admin")
        {
            List<Claim> claims = new()
            {
                 new Claim(ClaimTypes. Sid, "1"),
                    new Claim(ClaimTypes.Name, name),
                    new Claim(ClaimTypes.Actor,"1")
            };
            claims.AddSuperAdmin();
            claims.AddAllRoles();
            var identity = new ClaimsIdentity(claims.ToArray(), AppAuthenticationTypes.LocalAuthentication);
            return new ClaimsPrincipal(identity);
        }
        /// <summary>
        /// Processes a user login request, handling local admin, demo, Active Directory authentication, and MFA flows.
        /// </summary>
        /// <param name="loginReq">The login request details.</param>
        /// <returns>A <see cref="LoginRequest"/> object populated with the result of the login attempt.</returns>
        public async Task<LoginRequest> Login(LoginRequest loginReq)
        {
            var newUserState = _userStateService.CreateUserState(GetAnonymous(_httpContextAccessor.HttpContext?.Session.Id));
            newUserState.IPAddress = loginReq.IPAddress;


            AuthenticationState? authenticationState = null;

            //Set the current user from the HttpContext which gets it from the user's browser cookie
            CurrentUser = _httpContextAccessor?.HttpContext?.User;
            //Block impersonation logins from non superadmins
            if (loginReq.Impersonation
                && CurrentUser != null
                && !CurrentUser.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == UserRoles.SuperAdmin))
            {
                await _audit.Logon.AttemptedPersonation(loginReq.IPAddress);
                return loginReq.UnauthorizedImpersonation();
            }
            //If the user is impersonating then we want to remember who we were before
            if (loginReq.Impersonation)
            {
                //Prepare the UserState for the StateService to include the impersonator identity so
                //we can undo the impersonation later
                newUserState.Impersonator = CurrentUser;
                //Attach the impersonator to the login request so it can be used for later processing
                loginReq.ImpersonatorClaims = CurrentUser;
            }
            else
            {

                if (loginReq.Username.IsNullOrEmpty()) return loginReq.NoUsername();
            }
            //Pull the authentication settings from the database so we can check admin credentials
            using (var context = await _factory.CreateDbContextAsync())
            {

                var settings = await context.AuthenticationSettings.FirstOrDefaultAsync();
                if (settings == null) { Loggers.SystemLogger.Warning("AppAuthenticationStateProvider.Login: AuthenticationSettings are null from database."); }
                //Check admin credentials
                if (settings != null
                    && loginReq.Username != null
                    && loginReq.Username.Equals("admin", StringComparison.OrdinalIgnoreCase))
                {
                    var adminPass = _encryption.DecryptObject<string>(settings.AdminPassword);
                    if (loginReq.Password == adminPass)
                        authenticationState = await SetUser(GetLocalAdmin());
                    else
                        await _audit.Logon.AttemptedLogin(GetLocalAdmin(), loginReq.IPAddress);


                }
                //Check if we're in demo mode and this is a demo login
                else if (_applicationInfo.InDemoMode && settings != null
                    && loginReq.Username != null
                    && loginReq.Username.Equals("demo", StringComparison.OrdinalIgnoreCase) && loginReq.Password == "demo")
                {
                    authenticationState = await SetUser(GetDemoUser());

                }
                else
                {
                    try
                    {
                        //Login username is not "admin" or "demo" or we're not in demo mode, so we'll try active directory
                        var userClaim = await AttemptADLogin(newUserState, loginReq);

                        if (userClaim != null)
                        {
                            // Check that Duo is enabled and configured properly, also skip if impersonation
                            if (settings != null &&
                                settings.RequireMFA &&
                                settings.MFAType == MFAType.CiscoDuo &&
                                settings.DuoSettingsValid &&
                                !loginReq.Impersonation
                                )
                            {
                                //Duo is enabled, so we need to set up an MFA request
                                Loggers.SystemLogger.Information("AppAuthenticationStateProvider.Login: Initiating Duo MFA for user {UserName}.", loginReq.Username);
                                var mfaRRedirect = await PerformDuoAuthentication(loginReq);
                                if (mfaRRedirect.IsNullOrEmpty() && settings.DuoUnreachableBehavior == DuoUnreachableBehavior.Block)
                                {
                                    Loggers.SystemLogger.Warning("AppAuthenticationStateProvider.Login: Duo authentication for user {UserName} did not return a redirect URI, but DuoUnreachableBehavior is Block.", loginReq.Username);
                                }
                                //Settings are configured so
                                if (!mfaRRedirect.IsNullOrEmpty())
                                {
                                    var twostepState = GetAnonymous(loginReq.Id.ToString(), loginReq.MFAToken);
                                    var authResult = await SetUser(twostepState);
                                    newUserState.User = userClaim;
                                    _userStateService.SetMFAUserState(loginReq.MFAToken, newUserState, loginReq.ReturnUrl);
                                    authenticationState = authResult;
                                    return loginReq.DuoRequested(authenticationState);

                                }

                            }
                            else
                            {
                                //Duo is not enabled, or this is impersonation, proceed with post login processing
                                AppUser? userSettings = await GetUserSettings(context, userClaim);

                                if (userSettings != null
                                    && !loginReq.Impersonation
                                    && settings != null
                                    && settings.RequireMFA
                                    && settings.MFAType == MFAType.GoogleAuthenticator
                                    && userSettings.AuthenticatorSecret?.Decrypt<string>().IsNullOrEmpty() == false)
                                {
                                    Loggers.SystemLogger.Information("AppAuthenticationStateProvider.Login: Google Authenticator MFA required for user {UserName}.", userClaim.Identity.Name);
                                    var passcode = loginReq.MFAToken;
                                    loginReq.MFAToken = userSettings.AuthenticatorSecret.Decrypt<string>();
                                    if (passcode.IsNullOrEmpty() || !_googleAuthenticatorService.ValidateTwoFactorPIN(loginReq.MFAToken.ToSecureString(), passcode))
                                    {
                                        Loggers.SystemLogger.Warning("AppAuthenticationStateProvider.Login: Google Authenticator PIN validation failed for user {UserName}.", userClaim.Identity.Name);
                                        var twostepState = GetAnonymous(loginReq.Id.ToString(), loginReq.MFAToken);
                                        var authResult = await SetUser(twostepState);
                                        newUserState.User = userClaim;
                                        _userStateService.SetMFAUserState(loginReq.MFAToken, newUserState, loginReq.ReturnUrl);
                                        authenticationState = authResult;
                                        return loginReq.GoogleAuthenticatorRequested(authenticationState);
                                    }
                                }

                            }

                            //If active directory login/impersonation succeeded the userClaim will be popluated
                            if (userClaim.Identity?.IsAuthenticated == true)
                                //Set the user in the authentication provider
                                authenticationState = await SetUser(userClaim);
                        }
                    }
                    catch (DeniedLoginException)
                    {
                        return loginReq.DeniedLogin();
                    }

                }
            }
            if (authenticationState?.User != null)
            {
                //User claim processing is done so we can set the UserState with the new identity
                newUserState.User = authenticationState.User;

            }
            //Pass this state to the State Service for statefulness if it's populated
            if (newUserState.User != null)
                _userStateService.SetUserState(newUserState);


            //Return the authenticationstate
            if (authenticationState != null)
            {
                if (loginReq.AuthenticationResult == LoginResultStatus.OK) // This check might be redundant if only success path reaches here with non-null authState
                {
                    Loggers.SystemLogger.Information("AppAuthenticationStateProvider.Login: User {UserName} successfully logged in. Final ClaimsPrincipal Name: {PrincipalName}", loginReq.Username, authenticationState?.User?.Identity?.Name);
                }
                return loginReq.Success(authenticationState);
            }
            else
                return loginReq.BadCredentials();


        }

        private static async Task<AppUser?> GetUserSettings(IDatabaseContext context, ClaimsPrincipal? userClaim)
        {
            var sid = userClaim.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Sid)?.Value;
            var userSettings = await context.UserSettings.FirstOrDefaultAsync(x => x.UserGUID == sid);
            return userSettings;
        }

        /// <summary>
        /// Polls the active directory to either authenticate credentials or simply lookup
        /// a user depending on if the LoginRequest is for impersonation
        /// </summary>
        /// <param name="loginReq">The parameters passed from the login attempt</param>
        /// <returns>A fully processed ClaimsPrincipal representing the Web user data applied depending on
        /// database permission tables
        /// </returns>
        private async Task<ClaimsPrincipal?> AttemptADLogin(IApplicationUserState loginUser, LoginRequest loginReq)
        {
            Loggers.SystemLogger.Debug("AppAuthenticationStateProvider.AttemptADLogin: Attempting AD login for user {UserName}. Impersonation: {IsImpersonation}", loginReq.Username, loginReq.Impersonation);
            IADUser? user;
            if (!loginReq.Impersonation)
            {
                user = _directory.Authenticate(loginReq);
            }
            else
            {
                user = (await _directory.Users.FindUsersByStringAsync(loginReq.Username, true, true)).FirstOrDefault();
            }

            if (user == null)
            {
                Loggers.SystemLogger.Warning("AppAuthenticationStateProvider.AttemptADLogin: Active Directory user {UserName} not found or authentication failed.", loginReq.Username);
            }

            return await CreateDirectoryPrincipal(loginUser, user, loginReq);
        }

        private async Task<string> PerformDuoAuthentication(LoginRequest loginReq)
        {
            using (var context = await _factory.CreateDbContextAsync())
            {

                var settings = await context.AuthenticationSettings.FirstOrDefaultAsync();
                if (settings == null) throw new AppException("Could not get settings"); // Existing check, good.

                // Initiate the Duo authentication for a specific username

                // Get a Duo client
                Client duoClient = _duoClientProvider.GetDuoClient(loginReq.CallbackBaseUri + "/mfacallback");

                // Check if Duo seems to be healthy and able to service authentications.
                var isDuoHealthy = await duoClient.DoHealthCheck();
                if (!isDuoHealthy)
                {
                    if (settings.DuoUnreachableBehavior == DuoUnreachableBehavior.Block)
                    {
                        Loggers.SystemLogger.Error("AppAuthenticationStateProvider.PerformDuoAuthentication: Duo health check failed and DuoUnreachableBehavior is Block for user {UserName}.", loginReq.Username);
                        // Potentially throw or return empty to signify failure to redirect,
                        // which Login method will then handle. For now, just logging and returning empty.
                        return String.Empty;
                    }
                    if (settings.DuoUnreachableBehavior == DuoUnreachableBehavior.Bypass)
                    {
                        return String.Empty; //Bypass Duo
                    }
                }
                // Generate a random state value to tie the authentication steps together
                string state = Client.GenerateState();

                // Save the mfa state back to the login request
                loginReq.MFAToken = state;

                // Get the URI of the Duo prompt from the client.  This includes an embedded authentication request.
                string promptUri = duoClient.GenerateAuthUri(loginReq.Username, state);

                // Set up the redirect after successful mfa
                loginReq.MFARedirect = promptUri;


                return promptUri;



            }
        }

        /// <summary>
        /// Creates the foundation for the Active Directory user's ClaimsPrincipal. Then it passes to CreateDirectoryIdentity to
        /// Creates a <see cref="ClaimsPrincipal"/> for an Active Directory user, including transformed application roles and claims.
        /// </summary>
        /// <param name="loginUser">The application user state associated with the login attempt.</param>
        /// <param name="user">The <see cref="IADUser"/> from Active Directory.</param>
        /// <param name="loginReq">The original login request, used for context like impersonation.</param>
        /// <returns>A <see cref="ClaimsPrincipal"/> for the AD user, or null if the user is null.</returns>
        public async Task<ClaimsPrincipal?> CreateDirectoryPrincipal(IApplicationUserState loginUser, IADUser? user, LoginRequest? loginReq = null)
        {
            ClaimsPrincipal? principal = null;


            if (user != null)
            {
                principal = new ClaimsPrincipal(await CreateDirectoryIdentity(loginUser, user, loginReq));
            }

            return principal;
        }

        /// <summary>
        /// Creates a <see cref="ClaimsIdentity"/> for an Active Directory user, loading permissions and transforming them into application roles.
        /// </summary>
        /// <param name="loginUser">The application user state.</param>
        /// <param name="user">The Active Directory user.</param>
        /// <param name="loginReq">The login request details.</param>
        /// <returns>A <see cref="ClaimsIdentity"/> for the user, or null if user is null.</returns>
        public async Task<ClaimsIdentity?> CreateDirectoryIdentity(IApplicationUserState loginUser, IADUser user, LoginRequest? loginReq = null)
        {

            ClaimsIdentity identity;



            //Load privilege levels for user
            await _permissionHandler.LoadPermissions(loginUser, user);
            var userClaims = _permissionHandler.TransformUserRoles(loginUser, user, loginReq?.ImpersonatorClaims?.FindFirstValue(ClaimTypes.Sid));


            //All Claims transformations are complete create the new signed in user's identity
            identity = new ClaimsIdentity(userClaims, AppAuthenticationTypes.ActiveDirectoryAuthentication);


            return identity;

        }

        /// <summary>
        /// Sets the User AuthenticationState in the AuthenticationProvider
        /// </summary>
        /// <param name="claimsPrincipal"></param>
        /// <returns></returns>
        public Task<AuthenticationState> SetUser(ClaimsPrincipal claimsPrincipal)
        {
            this.CurrentUser = claimsPrincipal;
            var task = this.GetAuthenticationStateAsync();
            this.NotifyAuthenticationStateChanged(task);
            return task;
        }
        /// <summary>
        /// Logs out the current user by clearing their authentication state and notifying state change.
        /// </summary>
        /// <param name="claimsPrincipal">The <see cref="ClaimsPrincipal"/> of the user to log out.</param>
        /// <returns>The new <see cref="AuthenticationState"/> after logout (typically anonymous).</returns>
        public Task<AuthenticationState> Logout(ClaimsPrincipal claimsPrincipal)
        {
            _userStateService.RemoveUserState(claimsPrincipal);
            this.CurrentUser = GetAnonymous(_httpContextAccessor.HttpContext?.Session.Id);
            var task = this.GetAuthenticationStateAsync();
            this.NotifyAuthenticationStateChanged(task);
            return task;
        }


    }
}