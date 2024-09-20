using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using BLAZAM.Common.Data.Services;
using BLAZAM.Common.Exceptions;
using BLAZAM.Database.Context;
using BLAZAM.Database.Models;
using BLAZAM.Helpers;
using BLAZAM.Server.Helpers;
using BLAZAM.Services.Audit;
using BLAZAM.Services.Duo;
using BLAZAM.Session.Interfaces;
using DuoUniversal;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BLAZAM.Services
{
    /// <summary>
    /// Handles login/impersonate/logout of the browser HTTPContext Identity.
    /// This identity is stored it the Application's authentication cookie.
    /// </summary>
    public class AppAuthenticationStateProvider : AuthenticationStateProvider
    {
        public AppAuthenticationStateProvider(IAppDatabaseFactory factory,
            IActiveDirectoryContext directoy,
            PermissionApplicator permissionHandler,
            IApplicationUserStateService userStateService,
            IHttpContextAccessor ca,
            IDuoClientProvider dcp,
            IEncryptionService enc,
            AuditLogger audit,
            ApplicationInfo applicationInfo,
            NavigationManager nav)
        {
            _applicationInfo = applicationInfo;
            this._encryption = enc;
            this._directory = directoy;
            this._factory = factory;
            this._permissionHandler = permissionHandler;
            this._userStateService = userStateService;
            this._httpContextAccessor = ca;
            this._nav = nav;
            this.CurrentUser = this.GetAnonymous(ca.HttpContext?.Session.Id);

            this._duoClientProvider = dcp;
            this._audit = audit;
        }

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDuoClientProvider _duoClientProvider;
        private readonly AuditLogger _audit;
        private readonly ApplicationInfo _applicationInfo;
        private readonly IEncryptionService _encryption;
        private readonly NavigationManager _nav;
        private readonly IActiveDirectoryContext _directory;
        private readonly IAppDatabaseFactory _factory;
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
        private ClaimsPrincipal GetAnonymous(string? sessionId = null, string? mfaToken = null)
        {

            var identity = new ClaimsIdentity(new[]
           {
                    new Claim(ClaimTypes.Sid, sessionId.IsNullOrEmpty()==true?"0":sessionId),
                    new Claim(ClaimTypes.Name, "Anonymous"),
                    new Claim(ClaimTypes.Role, "Anonymous"),
                    new Claim(ClaimTypes.Actor,sessionId.IsNullOrEmpty()==true?"0":sessionId)
                }, null);
            if (mfaToken != null)
            {
                identity.AddClaim(new Claim(ClaimTypes.Rsa, mfaToken));
            }

            return new ClaimsPrincipal(identity);
        }
        private ClaimsPrincipal GetDemoUser()
        {
            List<Claim> claims = new List<Claim>
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
        private ClaimsPrincipal GetLocalAdmin(string name = "admin")
        {
            List<Claim> claims = new List<Claim>
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
        /// Processes a login request.
        /// </summary>
        /// <param name="loginReq">The authentication details and options for login</param>
        /// <returns>A fully processed AuthenticationState with all Claims and application permissions applied.</returns>
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
                if (loginReq == null) return loginReq.NoData();
                if (loginReq.Username.IsNullOrEmpty()) return loginReq.NoUsername();
            }
            //Pull the authentication settings from the database so we can check admin credentials
            using (var context = _factory.CreateDbContext())
            {

                var settings = context.AuthenticationSettings.FirstOrDefault();
                //Check admin credentials
                if (settings != null
                    && loginReq.Username != null
                    && loginReq.Username.Equals("admin", StringComparison.OrdinalIgnoreCase))
                {
                    var adminPass = _encryption.DecryptObject<string>(settings.AdminPassword);
                    if (loginReq.Password == adminPass)
                        authenticationState = await SetUser(this.GetLocalAdmin());
                    else
                        await _audit.Logon.AttemptedLogin(GetLocalAdmin(), loginReq.IPAddress);


                }
                //Check if we're in demo mode and this is a demo login
                else if (_applicationInfo.InDemoMode && settings != null
                    && loginReq.Username != null
                    && loginReq.Username.Equals("demo", StringComparison.OrdinalIgnoreCase) && loginReq.Password == "demo")
                {
                    authenticationState = await SetUser(this.GetDemoUser());

                }
                else
                {
                    try
                    {
                        //Login username is not "admin" or "demo" or we're not in demo mode, so we'll try active directory
                        var userClaim = await AttemptADLogin(newUserState, loginReq);

                        if (userClaim != null)
                        {
                            if (settings != null &&
                                settings.DuoEnabled &&
                                settings.DuoClientSecret != null &&
                                settings.DuoClientId != null &&
                                settings.DuoApiHost != null &&
                                !loginReq.Impersonation
                                )
                            {
                                var mfaRRedirect = await PerformDuoAuthentication(loginReq);
                                //Settings are configured so
                                if (!mfaRRedirect.IsNullOrEmpty())
                                {
                                    var twostepState = GetAnonymous(loginReq.Id.ToString(), loginReq.MFAToken);
                                    var authResult = await SetUser(twostepState);
                                    newUserState.User = userClaim;
                                    _userStateService.SetMFAUserState(loginReq.MFAToken, newUserState);
                                    authenticationState = authResult;
                                    return loginReq.MFARequested(authenticationState);

                                }
                                // var principal = await CreateDirectoryPrincipal(loginUser, user, loginReq);
                                //return ;
                                //Duo authentication requested

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
                //await _audit.Logon.Login(result.User);

                return loginReq.Success(authenticationState);
            }
            else
                return loginReq.BadCredentials();


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
            IADUser? user;
            if (!loginReq.Impersonation)
            {
                user = _directory.Authenticate(loginReq);

            }
            else
                user = _directory.Users.FindUsersByString(loginReq.Username, true, true).FirstOrDefault();


            return await CreateDirectoryPrincipal(loginUser, user, loginReq);


        }

        private async Task<string> PerformDuoAuthentication(LoginRequest loginReq)
        {
            using (var context = _factory.CreateDbContext())
            {

                var settings = context.AuthenticationSettings.FirstOrDefault();
                if (settings == null) throw new ApplicationException("Could not get settings");





                // Initiate the Duo authentication for a specific username

                // Get a Duo client
                Client duoClient = _duoClientProvider.GetDuoClient(loginReq.CallbackBaseUri + "/mfacallback");

                // Check if Duo seems to be healthy and able to service authentications.
                // If Duo were unhealthy, you could possibly send user to an error page, or implement a fail mode
                var isDuoHealthy = await duoClient.DoHealthCheck();
                if (!isDuoHealthy && settings.DuoUnreachableBehavior == DuoUnreachableBehavior.Bypass)
                {
                    return String.Empty;
                }
                // Generate a random state value to tie the authentication steps together
                string state = Client.GenerateState();

                loginReq.MFAToken = state;
                // Save the state and username in the session for later
                //HttpContext.Session.SetString(STATE_SESSION_KEY, state);
                //HttpContext.Session.SetString(USERNAME_SESSION_KEY, username);

                // Get the URI of the Duo prompt from the client.  This includes an embedded authentication request.
                string promptUri = duoClient.GenerateAuthUri(loginReq.Username, state);
                loginReq.MFARedirect = promptUri;
                // Redirect the user's browser to the Duo prompt.
                // The Duo prompt, after authentication, will redirect back to the configured Redirect URI to complete the authentication flow.
                // In this example, that is /duo_callback, which is implemented in Callback.cshtml.cs.
                // return new RedirectResult(promptUri);

                return promptUri;



            }
        }

        /// <summary>
        /// Creates the foundation for the Active Directory user's ClaimsPrincipal. Then it passes to CreateDirectoryIdentity to
        /// actually make the ClaimsIdentity inside the principal.
        /// </summary>
        /// <param name="user">The user found in Active Directory that matches the LoginRequest Username</param>
        /// <param name="loginReq">The parameters passed from the login attempt</param>
        /// <returns>A fully processed ClaimsPrincipal representing the Web user with data applied depending on
        /// database permission tables</returns>
        private async Task<ClaimsPrincipal?> CreateDirectoryPrincipal(IApplicationUserState loginUser, IADUser? user, LoginRequest loginReq)
        {
            ClaimsPrincipal? principal = null;


            if (user != null)
            {
                principal = new ClaimsPrincipal(await CreateDirectoryIdentity(loginUser, user, loginReq));
            }

            return principal;
        }

        /// <summary>
        /// Processes the Active Directory Users permissions applied in the database and maps the roles to be assigned
        /// </summary>
        /// <param name="user">The Active Directory User to create an identity for</param>
        /// <param name="loginReq">The parameteres passed from the login attempt</param>
        /// <returns>A fully processed ClaimsIdentity representing the user in Active Directory with data applied depending on
        /// database permission tables</returns>
        private async Task<ClaimsIdentity?> CreateDirectoryIdentity(IApplicationUserState loginUser, IADUser user, LoginRequest loginReq)
        {

            var identity = new ClaimsIdentity();



            //Load privilege levels for user
            await _permissionHandler.LoadPermissions(loginUser, user);
            var userRoles = TransformUserRoles(loginUser);
            //TransformUserRoles returns an empty list if the user has no login rights
            if (userRoles.Count < 1)
                throw new DeniedLoginException();

            //Build the base of the ClaimIdentity
            List<Claim> claims = new()
                {
                            new Claim(ClaimTypes.Sid, user.SID.ToSidString()),

                        };
            if (user.DisplayName != null)
            {
                claims.Add(new Claim(ClaimTypes.Name, user.DisplayName));
            }
            else if (user.SamAccountName != null)
            {
                claims.Add(new Claim(ClaimTypes.Name, user.SamAccountName));

            }
            if (user.UserPrincipalName != null)
                claims.Add(new Claim(ClaimTypes.WindowsAccountName, user.SamAccountName));
            if (user.GivenName != null)
                claims.Add(new Claim(ClaimTypes.GivenName, user.GivenName));
            if (user.Surname != null)
                claims.Add(new Claim(ClaimTypes.Surname, user.Surname));
            if (user.Email != null)
                claims.Add(new Claim(ClaimTypes.Email, user.Email));

            if (loginReq.Impersonation)
            {
                //Handle Impersonated login
                claims.Add(new Claim(ClaimTypes.UserData, "impersonated"));
                //Set the impersonators SID to the actor claim type so we know who to unimpersonate back to
                claims.Add(new Claim(ClaimTypes.Actor, loginReq.ImpersonatorClaims.FindFirstValue(ClaimTypes.Sid)));

            }
            else
            {
                //This sign in is not impersonated, so we use the users SID we got from Active Directory above
                claims.Add(new Claim(ClaimTypes.Actor, user.SID.ToSidString()));

            }

            //All Claims transformations are complete create the new signed in user's identity
            identity = new ClaimsIdentity(claims, AppAuthenticationTypes.ActiveDirectoryAuthentication);
            //Inject the appended transformations for [Authorized()] usage
            userRoles.ForEach(ur =>
            {
                identity.AddClaim(ur);
            });

            return identity;

        }
        /// <summary>
        /// Uses the Active Directory user who logged in and transforms
        /// their identity to the applications ClaimRoles based
        /// on the permissions set in the database
        /// </summary>
        /// <param name="user">The Active Directory user who authenticated</param>
        /// <returns>A list of Claim Roles that the user has been privileged</returns>
        private List<Claim> TransformUserRoles(IApplicationUserState user)
        {

            List<Claim> userRoles = new();

            if (user.PermissionDelegates.Any(p => p.IsSuperAdmin))
            {
                userRoles.AddSuperAdmin();
                userRoles.AddAllRoles();

            }
            else
            {
                if (user.HasUserPrivilege)
                {
                    userRoles.Add(new Claim(ClaimTypes.Role, UserRoles.SearchUsers));
                }
                if (user.HasCreateUserPrivilege)
                {
                    userRoles.Add(new Claim(ClaimTypes.Role, UserRoles.CreateUsers));
                }
                if (user.HasGroupPrivilege)
                {
                    userRoles.Add(new Claim(ClaimTypes.Role, UserRoles.SearchGroups));
                }
                if (user.HasCreateGroupPrivilege)
                {
                    userRoles.Add(new Claim(ClaimTypes.Role, UserRoles.CreateGroups));
                }
                if (user.HasOUPrivilege)
                {
                    userRoles.Add(new Claim(ClaimTypes.Role, UserRoles.SearchOUs));
                }
                if (user.HasCreateOUPrivilege)
                {
                    userRoles.Add(new Claim(ClaimTypes.Role, UserRoles.CreateOUs));
                }
                if (user.HasComputerPrivilege)
                {
                    userRoles.Add(new Claim(ClaimTypes.Role, UserRoles.SearchComputers));
                }
                if (user.HasBitLockerPrivilege)
                {
                    userRoles.Add(new Claim(ClaimTypes.Role, UserRoles.SearchBitLocker));
                }

            }
            return userRoles;
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
        /// This may not be entirely necessary the way I am implementing authentication and authorization
        /// Though, this likely is needed to remove the cookie to actually sign out.
        /// </summary>
        /// <param name="claimsPrincipal"></param>
        /// <returns></returns>
        public Task<AuthenticationState> Logout(ClaimsPrincipal claimsPrincipal)
        {
            _userStateService.RemoveUserState(claimsPrincipal);
            this.CurrentUser = this.GetAnonymous(_httpContextAccessor.HttpContext?.Session.Id);
            var task = this.GetAuthenticationStateAsync();
            this.NotifyAuthenticationStateChanged(task);
            return task;
        }


    }
}