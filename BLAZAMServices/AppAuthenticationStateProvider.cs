using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data.Services;
using BLAZAM.Database.Context;
using BLAZAM.Database.Models;
using BLAZAM.Database.Models.User;
using BLAZAM.Helpers;
using BLAZAM.Logger;
using BLAZAM.Services.Audit;
using BLAZAM.Services.Background;
using BLAZAM.Services.Duo;
using BLAZAM.Services.Exceptions;
using BLAZAM.Session;
using BLAZAM.Session.Interfaces;
using DuoUniversal;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;


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
            IActiveDirectoryContext directory,
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
                {
                    options.ExpireTimeSpan = TimeSpan.FromMinutes((double)DatabaseCache.AuthenticationSettings.SessionTimeout);
                }
                else
                {
                    options.ExpireTimeSpan = TimeSpan.FromSeconds(10);
                }

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
            List<Claim> claims =
            [
                new Claim(ClaimTypes.Sid, "2"),
                new Claim(ClaimTypes.Name, "Demo"),
                new Claim(ClaimTypes.Actor, "2")
            ];
            claims.AddSuperAdmin();
            claims.AddAllRoles();
            var identity = new ClaimsIdentity(claims.ToArray(), AppAuthenticationTypes.LocalAuthentication);
            return new ClaimsPrincipal(identity);
        }
        private static ClaimsPrincipal GetLocalAdmin(string name = "admin")
        {
            List<Claim> claims =
            [
                 new Claim(ClaimTypes. Sid, "1"),
                    new Claim(ClaimTypes.Name, name),
                    new Claim(ClaimTypes.Actor,"1")
            ];
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
            CurrentUser = _httpContextAccessor.HttpContext?.User;

            if (IsUnauthorizedImpersonation(loginReq))
            {
                await _audit.Logon.AttemptedPersonation(loginReq.IPAddress);
                return loginReq.UnauthorizedImpersonation();
            }

            if (loginReq.Impersonation)
            {
                SetImpersonationState(newUserState, loginReq);
            }
            else if (loginReq.Username.IsNullOrEmpty())
            {
                return loginReq.NoUsername();
            }

            using var context = await _factory.CreateDbContextAsync();
            var settings = await context.AuthenticationSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                Loggers.SystemLogger.Warning("AppAuthenticationStateProvider.Login: AuthenticationSettings are null from database.");
            }

            authenticationState = await HandleLoginByType(loginReq, newUserState, context, settings);

            if (authenticationState?.User != null)
            {
                newUserState.User = authenticationState.User;
            }
            if (newUserState.User != null)
            {
                _userStateService.SetUserState(newUserState);
            }

            if (authenticationState != null)
            {
                if (loginReq.AuthenticationResult == LoginResultStatus.OK)
                {
                    Loggers.SystemLogger.Information("AppAuthenticationStateProvider.Login: User {UserName} successfully logged in. Final ClaimsPrincipal Name: {PrincipalName}", loginReq.Username, authenticationState.User?.Identity?.Name);
                    return loginReq.Success(authenticationState);

                }
                return loginReq;
            }
            else
            {
                return loginReq.BadCredentials();
            }
        }

        private bool IsUnauthorizedImpersonation(LoginRequest loginReq)
        {
            return loginReq.Impersonation
                && CurrentUser != null
                && !CurrentUser.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == UserRoles.SuperAdmin);
        }

        private void SetImpersonationState(IApplicationUserState newUserState, LoginRequest loginReq)
        {
            newUserState.Impersonator = CurrentUser;
            loginReq.ImpersonatorClaims = CurrentUser;
        }

        private async Task<AuthenticationState?> HandleLoginByType(LoginRequest loginReq, IApplicationUserState newUserState, IDatabaseContext context, AuthenticationSettings? settings)
        {
            if (IsLocalAdminLogin(loginReq, settings))
            {
                return await HandleLocalAdminLogin(loginReq, settings);
            }
            else if (IsDemoLogin(loginReq, settings))
            {

                loginReq.AuthenticationResult = LoginResultStatus.OK;
                return await SetUser(GetDemoUser());
            }
            else
            {
                return await HandleActiveDirectoryLogin(loginReq, newUserState, context, settings);
            }
        }

        private bool IsLocalAdminLogin(LoginRequest loginReq, AuthenticationSettings? settings)
        {
            return settings != null
                && loginReq.Username != null
                && loginReq.Username.Equals("admin", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<AuthenticationState?> HandleLocalAdminLogin(LoginRequest loginReq, AuthenticationSettings settings)
        {
            var adminPass = _encryption.DecryptObject<string>(settings.AdminPassword);
            if (loginReq.Password == adminPass)
            {
                loginReq.AuthenticationResult = LoginResultStatus.OK;
                return await SetUser(GetLocalAdmin());
            }
            else
            {
                await _audit.Logon.AttemptedLogin(GetLocalAdmin(), loginReq.IPAddress);
            }
            return null;
        }

        private bool IsDemoLogin(LoginRequest loginReq, AuthenticationSettings? settings)
        {
            return _applicationInfo.InDemoMode && settings != null
                && loginReq.Username != null
                && loginReq.Username.Equals("demo", StringComparison.OrdinalIgnoreCase)
                && loginReq.Password == "demo";
        }

        private async Task<AuthenticationState?> HandleDuoMFA(LoginRequest loginReq, IApplicationUserState newUserState, ClaimsPrincipal userClaim)
        {
            var mfaRedirect = await PerformDuoAuthentication(loginReq);
            if (!mfaRedirect.IsNullOrEmpty())
            {
                var twostepState = GetAnonymous(loginReq.Id.ToString(), loginReq.MFAToken);
                var authResult = await SetUser(twostepState);
                newUserState.User = userClaim;
                _userStateService.SetMFAUserState(MfaType.CiscoDuo, loginReq.MFAToken, newUserState, loginReq.ReturnUrl);
                loginReq.MFARedirect = mfaRedirect;
                loginReq.AuthenticationState = authResult;
                loginReq.AuthenticationResult = LoginResultStatus.DuoRequested;
                //throw new DuoMFARequestedException(loginReq.DuoRequested(authResult,mfaRedirect));
                return authResult;
            }
            return null;
        }
        private async Task<AuthenticationState?> HandleGoogleAuthenticatorMFA(LoginRequest loginReq, IApplicationUserState newUserState, ClaimsPrincipal userClaim, AppUser? userSettings)
        {
            var passcode = loginReq.MFAToken;
            loginReq.MFAToken = userSettings.AuthenticatorSecret.Decrypt<string>();
            if (passcode.IsNullOrEmpty() || !_googleAuthenticatorService.ValidateTwoFactorPIN(loginReq.MFAToken.ToSecureString(), passcode))
            {
                var twostepState = GetAnonymous(loginReq.Id.ToString(), loginReq.MFAToken);
                var authResult = await SetUser(twostepState);
                newUserState.User = userClaim;
                _userStateService.SetMFAUserState(MfaType.GoogleAuthenticator, loginReq.MFAToken, newUserState, loginReq.ReturnUrl);
                throw new GoogleMFARequestedException(loginReq.GoogleAuthenticatorRequested(authResult));
            }
            return null;
        }
        private async Task<AuthenticationState?> HandleActiveDirectoryLogin(LoginRequest loginReq, IApplicationUserState newUserState, IDatabaseContext context, AuthenticationSettings? settings)
        {
            try
            {
                var userClaim = await AttemptADLogin(newUserState, loginReq);
                if (userClaim == null)
                {
                    return null;
                }

                if (ShouldPerformDuoMFA(settings, loginReq))
                {
                    var duoResult = await HandleDuoMFA(loginReq, newUserState, userClaim);
                    if (duoResult != null)
                    {
                        return duoResult;
                    }
                }
                else
                {
                    var userSettings = await GetUserSettings(context, userClaim);
                    if (ShouldPerformGoogleAuthenticatorMFA(userSettings, loginReq, settings))
                    {
                        var googleAuthResult = await HandleGoogleAuthenticatorMFA(loginReq, newUserState, userClaim, userSettings);
                        if (googleAuthResult != null)
                        {
                            return googleAuthResult;
                        }
                    }
                }

                if (userClaim.Identity?.IsAuthenticated == true)
                {
                    return await SetUser(userClaim);
                }
            }
            catch (DeniedLoginException)
            {
                return loginReq.DeniedLogin().AuthenticationState;
            }
            return null;
        }

        private bool ShouldPerformDuoMFA(AuthenticationSettings? settings, LoginRequest loginReq)
        {
            return settings != null &&
                settings.RequireMFA &&
                settings.MFAType == MfaType.CiscoDuo &&
                settings.DuoSettingsValid &&
                !loginReq.Impersonation;
        }

        private bool ShouldPerformGoogleAuthenticatorMFA(AppUser? userSettings, LoginRequest loginReq, AuthenticationSettings? settings)
        {
            return userSettings != null
                && !loginReq.Impersonation
                && settings != null
                && settings.RequireMFA
                && settings.MFAType == MfaType.GoogleAuthenticator
                && userSettings.AuthenticatorSecret?.Decrypt<string>().IsNullOrEmpty() == false;
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
            else
            {
                if (user.LockedOut)
                {
                    throw new LockedOutUserException();
                }
            }

            return await CreateDirectoryPrincipal(loginUser, user, loginReq);
        }

        private async Task<string> PerformDuoAuthentication(LoginRequest loginReq)
        {
            using var context = await _factory.CreateDbContextAsync();

            var settings = await context.AuthenticationSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                throw new AppException("Could not get settings"); // Existing check, good.
            }

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