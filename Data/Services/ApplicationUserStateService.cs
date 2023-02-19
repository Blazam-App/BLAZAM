using BLAZAM.Common.Data.Database;
using BLAZAM.Common.Data.Services;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BLAZAM.Server.Data.Services
{
    /// <summary>
    /// A stateful "session" store for the application's user session state. This class is a "hack" for Blazor Server
    /// to allow it to behave as if there is a persistent user web session between reloads on the client. Using this
    /// class data can be persisted over the course of an entire user's login session. In the context of this app,
    /// this class's primary purpose is to cache the user permission for logged in ActiveDirectory users.
    /// Each logged in user's ClaimsPrincipal is cached at login, retrieved on reload, and removed either on logout,
    /// or after 3x the Timeout set in the AuthenticationSettings in the Database. Of note: On webapp restart, if a
    /// valid ClaimsPrincipal still exists in the user's browser cookies for a logged in user when they reload any 
    /// page the cache is updated with the missing ClaimsPrincipal.
    /// </summary>
    public class ApplicationUserStateService : IApplicationUserStateService
    {
        private IHttpContextAccessor HttpContextAccessor { get; set; }

        private IDbContextFactory<DatabaseContext> Factory;

        private int? Timeout { get; set; }
        /// <summary>
        /// Called when a new UserState is added to the cache. The new user state is passed along with the event.
        /// </summary>
        public AppEvent<IApplicationUserState> UserStateAdded { get; set; }
        /// <summary>
        /// A cached list of user states for logged in users. This allows easy, cached access to the users permissions and DirectryEntry
        /// </summary>
        public IList<IApplicationUserState> UserStates { get; private set; } = new List<IApplicationUserState>();
        private Timer t;
        /// <summary>
        /// A service to provide stateful user session data storage for runtime. Caches all logged in users.
        /// Raises a UserStateAdded event when a new state is added to the cache for processing in other modules.
        /// </summary>
        /// <param name="httpContextAccessor">An HTTP Context Accessor to get the current ClaimsPrincipal of the current session.
        /// This Principal is persisted via the browser authentication cookie</param>
        /// <param name="factory">Database Context Factory for accessing the Authentication Setting - SessionTimeout</param>
        public ApplicationUserStateService(IHttpContextAccessor httpContextAccessor, IDbContextFactory<DatabaseContext> factory)
        {

            HttpContextAccessor = httpContextAccessor;
            Factory = factory;
            t = new Timer(Tick, UserStates, 60000, 60000);
            Task.Run(async () =>
            {
                Timeout = (await factory.CreateDbContextAsync()).AuthenticationSettings.FirstOrDefault()?.SessionTimeout;

            });
        }
        /// <summary>
        /// Ticker to check for stale user states that haven't been accessed for
        /// 3X the sessionTimeout set for the application in the database
        /// </summary>
        /// <param name="state">The UserStates List object</param>
        private void Tick(object? state)
        {
            if (state is List<IApplicationUserState> userStates)
            {
                var temp = new List<IApplicationUserState>(userStates);
                var now = DateTime.UtcNow;
                temp.ForEach(x =>
                {
                    if ((now - x.LastAccessed).TotalMinutes > Timeout * 3)
                    {
                        userStates.Remove(x);

                    }
                });
            }
        }
        /// <summary>
        /// Gets the current UserState cached object of the currently 
        /// logged in user, referenced by the users browser authentication cookie
        /// </summary>
        public IApplicationUserState? CurrentUserState
        {
            get
            {
                try
                {
                    return GetUserState(HttpContextAccessor.HttpContext?.User);
                }
                catch (Exception ex)
                {
                    return null;
                }

            }
        }

        public string CurrentUsername
        {
            get
            {
                try
                {
                    var cu = CurrentUserState;
                    if (cu != null)
                    {
                        if (cu.User.FindFirstValue(ClaimTypes.UserData) != null)
                        {
                            return cu.Impersonator.Identity.Name;
                        }
                        return cu.User.Identity.Name;
                    }
                }
                catch
                {

                }
                return "";
            }
        }


        /// <summary>
        /// Get the matching cached user state for a given ClaimsPrincipal. 
        /// This principal is usually attained via the browser authentication cookie.
        /// </summary>
        /// <param name="userClaim">The users ClaimsPrincipal to match against.</param>
        /// <returns></returns>
        public IApplicationUserState? GetUserState(ClaimsPrincipal userClaim)
        {
            //Null check
            if (userClaim == null) return null;

            //Prepare empty application user state in case we don't find or make one
            IApplicationUserState existingState;

            //Search existing user stated for matching principals
            existingState = UserStates.Where(s => s.User.FindFirstValue(ClaimTypes.Sid) == userClaim.FindFirstValue(ClaimTypes.Sid)
            && s.User.FindFirstValue(ClaimTypes.Actor) == userClaim.FindFirstValue(ClaimTypes.Actor)).FirstOrDefault();

            //Search null check
            if (existingState == null)
            {

                //if (!userClaim.Identity.IsAuthenticated) return null;
                //Create a new cached state since the one we're looking for appears to be missing
                existingState = new ApplicationUserState { User = userClaim };
                AddUserState(existingState);

            }

            //Update the last accessed time since this code only runs when this specific user is logged in and making requests
            existingState.LastAccessed = DateTime.UtcNow;


            return existingState;
        }
        private void AddUserState(IApplicationUserState state)
        {
            state.DbFactory = Factory;
            UserStates.Add(state);
            //Invoke event so Active Directory can populate DirectoryUser if required
            UserStateAdded?.Invoke(state);
        }
        public void SetUserState(IApplicationUserState state)
        {
            if (state != null)
                if (UserStates.Count == 0 || !UserStates.Contains(state))
                {
                    AddUserState(state);
                }


        }
        public void RemoveUserState(IApplicationUserState state)
        {
            if (state != null)
                if (UserStates.Count > 0 && UserStates.Contains(state))
                    UserStates.Remove(state);


        }

        public void RemoveUserState(ClaimsPrincipal currentUser) => RemoveUserState(GetUserState(currentUser));
    }
}
