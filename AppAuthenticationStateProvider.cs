using BLAZAM.Common.Data;
using BLAZAM.Common.Data.Services;
using BLAZAM.Server.Data.Services;
using BLAZAM.Server.Data.Services.Duo;
using BLAZAM.Server.Data;
using BLAZAM.Server.Pages.Users;
using BLAZAM.Server.Shared.UI.Settings.Permissions;
using DuoUniversal;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using BLAZAM.Common.Helpers;
using BLAZAM.Common.Data.Database;
using BLAZAM.Common.Data.ActiveDirectory.Interfaces;

namespace BLAZAM
{
    /// <summary>
    /// Handles login/impersonate/logout of the browser HTTPContext Identity.
    /// This identity is stored it the Application's authentication cookie.
    /// </summary>
    public class AppAuthenticationStateProvider : AuthenticationStateProvider
    {
        public AppAuthenticationStateProvider(IDbContextFactory<DatabaseContext> factory,
            IActiveDirectory directoy,
            PermissionHandler permissionHandler,
            IApplicationUserStateService userStateService,
            IHttpContextAccessor ca,
            IDuoClientProvider dcp)
        {
            this._directory = directoy;
            this._factory = factory;
            this._permissionHandler = permissionHandler;
            this._userStateService = userStateService;
            this.CurrentUser = this.GetAnonymous();
            this._httpContextAccessor = ca;
            this._duoClientProvider = dcp;
        }

        private IHttpContextAccessor _httpContextAccessor;
        private IDuoClientProvider _duoClientProvider;
        private IActiveDirectory _directory;
        private IDbContextFactory<DatabaseContext> _factory;
        private PermissionHandler _permissionHandler;

        private IApplicationUserStateService _userStateService;


        private ClaimsPrincipal? CurrentUser;
        private ApplicationUserState _newUserState = new();

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var task = Task.FromResult(new AuthenticationState(this.CurrentUser));

            return task;
        }

        /// <summary>
        /// Creates an annonymous ClaimsPrincipal to handle authentication
        /// before login.
        /// </summary>
        /// <returns>An unauthenticated annonymous User ClaimsPrincipal</returns>
        private ClaimsPrincipal GetAnonymous()
        {
            var identity = new ClaimsIdentity(new[]
           {
                    new Claim(ClaimTypes.Sid, "0"),
                    new Claim(ClaimTypes.Name, "Anonymous"),
                    new Claim(ClaimTypes.Role, "Anonymous"),
                    new Claim(ClaimTypes.Actor,"0")
                }, null);

            return new ClaimsPrincipal(identity);
        }

        private ClaimsPrincipal GetLocalAdmin()
        {

            var identity = new ClaimsIdentity(new[]
            {
                    new Claim(ClaimTypes. Sid, "1"),
                    new Claim(ClaimTypes.Name, "admin"),
                    new Claim(ClaimTypes.Role, UserRoles.SuperAdmin ),
                    new Claim(ClaimTypes.Actor,"1")
                }, AppAuthenticationTypes.LocalAuthentication);
            return new ClaimsPrincipal(identity);
        }
        /// <summary>
        /// Processes a login request.
        /// </summary>
        /// <param name="loginReq">The authentication details and options for login</param>
        /// <returns>A fully processed AuthenticationState with all Claims and application permissions applied.</returns>
        public async Task<AuthenticationState?> Login(LoginRequest loginReq)
        {

            if (loginReq != null && loginReq.Username != null && loginReq.Username != "")
            {
                AuthenticationState? result = null;

                //Set the current user from the HttpContext which gets it from the user's browser cookie
                CurrentUser = _httpContextAccessor?.HttpContext?.User;
                //Block impersonation logins from non superadmins
                if (loginReq.Impersonation && CurrentUser != null && !CurrentUser.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == UserRoles.SuperAdmin)) return new AuthenticationState(this.CurrentUser);
                //If the user is impersonating then we want to remember who we were before
                if (loginReq.Impersonation)
                {
                    //Prepare the UserState for the StateService to include the impersonator identity so
                    //we can undo the impersonation later
                    _newUserState.Impersonator = CurrentUser;
                    //Attach the impersonator to the login request so it can be used for later processing
                    loginReq.ImpersonatorClaims = CurrentUser;
                }
                //Pull the authentication settings from the database so we can check admin credentials
                var settings = _factory.CreateDbContext().AuthenticationSettings.FirstOrDefault();
                //Check admin credentials
                if (settings != null && loginReq.Username.Equals("admin", StringComparison.OrdinalIgnoreCase))
                {
                    if (loginReq.Password == settings.AdminPassword)
                        result = await SetUser(this.GetLocalAdmin());

                }
                else
                {
                    //Login username is not "admin", so we'll try active directory
                    var userClaim = await AttemptADLogin(loginReq);
                    //If active directory login/impersonation succeeded the userClaim will be popluated
                    if (userClaim != null && userClaim.Identity.IsAuthenticated)
                        //Set the user in the authentication provider
                        result = await SetUser(userClaim);
                }

                //User claim processing is done so we can set the UserState with the new identity
                _newUserState.User = result?.User;

                //Pass this state to the State Service for statefulness if it's populated
                if (_newUserState.User != null)
                    _userStateService.SetUserState(_newUserState);

                //Return the authenticationstate
                if (result != null)
                    return result;
                else
                    return null;
            }
            return null;

        }

        /// <summary>
        /// Polls the active directory to either authenticate credentials or simply lookup
        /// a user depending on if the LoginRequest is for impersonation
        /// </summary>
        /// <param name="loginReq">The parameteres passed from the login attempt</param>
        /// <returns>A fully processed ClaimsPrincipal representing the Web user data applied depending on
        /// database permission tables
        /// </returns>
        private async Task<ClaimsPrincipal?> AttemptADLogin(LoginRequest loginReq)
        {
            IADUser user;
            if (!loginReq.Impersonation)
            {
                user = _directory.Authenticate(loginReq);
                /*
                 * Duo Authentication if i ever get it working with blazor server
                if (user != null)
                {
                    //Check if we need to perform MFA
                    using (var context = _factory.CreateDbContext())
                    {
                        //Get settings from DB
                        var authSettings = context.AuthenticationSettings.FirstOrDefault();
                        if (authSettings != null &&
                            authSettings.DuoClientSecret != null &&
                            authSettings.DuoClientId != null &&
                            authSettings.DuoApiHost != null
                            )
                        {
                            //Settings are configured so
                            if (!(await PerformDuoAuthentication(loginReq)))
                            {
                                //Duo authentication failed;

                                return null;

                            }
                            //Duo authentication succeeded

                        }
                    }
                }
                */
            }
            else
                user = _directory.Users.FindUsersByString(loginReq.Username, true, true).FirstOrDefault();


            return await CreateDirectoryPrincipal(user, loginReq);
          

        }

        private async Task<bool> PerformDuoAuthentication(LoginRequest loginReq)
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
            //HttpContext.Session.SetString(STATE_SESSION_KEY, state);
            //HttpContext.Session.SetString(USERNAME_SESSION_KEY, username);

            // Get the URI of the Duo prompt from the client.  This includes an embedded authentication request.
            string promptUri = duoClient.GenerateAuthUri(loginReq.Username, state);

            // Redirect the user's browser to the Duo prompt.
            // The Duo prompt, after authentication, will redirect back to the configured Redirect URI to complete the authentication flow.
            // In this example, that is /duo_callback, which is implemented in Callback.cshtml.cs.
            // return new RedirectResult(promptUri);
            return true;
        }

        /// <summary>
        /// Creates the foundation for the Active Directory user's ClaimsPrincipal. Then it passes to CreateDirectoryIdentity to
        /// actually make the ClaimsIdentity inside the principal.
        /// </summary>
        /// <param name="user">The user found in Active Directory that matches the LoginRequest Username</param>
        /// <param name="loginReq">The parameteres passed from the login attempt</param>
        /// <returns>A fully processed ClaimsPrincipal representing the Web user with data applied depending on
        /// database permission tables</returns>
        private async Task<ClaimsPrincipal?> CreateDirectoryPrincipal(IADUser? user, LoginRequest loginReq)
        {
            ClaimsPrincipal principal = null;


            if (user != null)
            {
                principal = new ClaimsPrincipal(await CreateDirectoryIdentity(user, loginReq));
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
        private async Task<ClaimsIdentity?> CreateDirectoryIdentity(IADUser user, LoginRequest loginReq)
        {
            
            var identity = new ClaimsIdentity();

            _newUserState.DirectoryUser = user;
            //Load privilege levels for user
            await _permissionHandler.LoadPermissions(user);

            var userRoles = TransformUserRoles(user);
            //TransformUserRoles returns an empty list if the user has no login rights
            if (userRoles.Count > 0)
            {
                //Build the base of the ClaimIdentity
#pragma warning disable CS8604 // Possible null reference argument.
                List<Claim> claims = new List<Claim>
                   {
                            new Claim(ClaimTypes.Sid, user.SID.ToSidString()),
                           new Claim(ClaimTypes.Name, user.DisplayName),
                           new Claim(ClaimTypes.GivenName, user.GivenName),
                           new Claim(ClaimTypes.Surname, user.Surname),
                        };
#pragma warning restore CS8604 // Possible null reference argument.
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
            }
            return identity;

        }
        /// <summary>
        /// Uses the Active Directory user who logged in and transforms
        /// their identity to the applications ClaimRoles based
        /// on the permissions set in the database
        /// </summary>
        /// <param name="user">The Active Directory user who authenticated</param>
        /// <returns>A list of Claim Roles that the user has been privileged</returns>
        private  List<Claim> TransformUserRoles(IADUser? user)
        {
            List<Claim> userRoles = new();

            if (user.PrivilegeLevels.Any(p => p.IsSuperAdmin))
            {
                userRoles.Add(new Claim(ClaimTypes.Role, UserRoles.SuperAdmin));

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
                    userRoles.Add(new Claim(ClaimTypes.Role, UserRoles.Computers));
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
        /// This may not be entirely neccessary the way I am implementin authentication and authorization
        /// Though, this likey is needed to remove the cookie to actually signout?
        /// </summary>
        /// <param name="claimsPrincipal"></param>
        /// <returns></returns>
        public Task<AuthenticationState> Logout(ClaimsPrincipal claimsPrincipal)
        {
            _userStateService.RemoveUserState(claimsPrincipal);
            this.CurrentUser = this.GetAnonymous();
            var task = this.GetAuthenticationStateAsync();
            this.NotifyAuthenticationStateChanged(task);
            return task;
        }


    }
}