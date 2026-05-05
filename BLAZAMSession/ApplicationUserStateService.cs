using BLAZAM.Database.Models;
using BLAZAM.Global;
using BLAZAM.Helpers; // Added for GetAppHashCode
using BLAZAM.Logger;
using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims; // Added

namespace BLAZAM.Session
{
    /// <summary>
    /// Manages in-memory user session states for the Blazor Server application, providing a way to cache user-specific data and permissions across interactions. It handles state creation, retrieval, and cleanup of stale sessions.
    /// </summary>
    public class ApplicationUserStateService : IApplicationUserStateService
    {


        private static readonly object _mfaQueueLock = new();


        private static readonly object _userStatesLock = new();

        /// <summary>
        /// Gets the singleton instance of the ApplicationUserStateService.
        /// </summary>
        public static IApplicationUserStateService Instance { get; private set; }

        private IHttpContextAccessor _httpContextAccessor { get; set; }
        private readonly IAppDatabaseFactory _factory;
        private int? Timeout { get; set; }
        private readonly List<MFARequest> _mfaLoginQueue = [];

        /// <summary>Event triggered when a new <see cref="IApplicationUserState"/> is added to the cache. Primarily for internal use or advanced scenarios.</summary>
        public AppDelegate<IApplicationUserState> UserStateAdded { get; set; }

        /// <summary>Event triggered when an <see cref="IApplicationUserState"/> is removed from the cache, either due to timeout or explicit logout.</summary>
        public AppDelegate<IApplicationUserState> OnUserStateRemoved { get; set; }

        /// <summary>Gets the list of currently cached <see cref="IApplicationUserState"/> objects. Use with caution; direct manipulation is not recommended.</summary>
        public IList<IApplicationUserState> UserStates { get; private set; } = [];

        private readonly Timer? t;

        /// <summary>Initializes a new instance of the <see cref="ApplicationUserStateService"/> class.</summary> 
        /// <param name="httpContextAccessor">Accessor for the current HTTP context, used to retrieve the user's ClaimsPrincipal.</param> 
        /// <param name="factory">Factory for creating database context instances.</param> 
        /// <exception cref="ArgumentNullException">Thrown if httpContextAccessor or factory is null.</exception>
        public ApplicationUserStateService(IHttpContextAccessor httpContextAccessor, IAppDatabaseFactory factory)
        {
            ArgumentNullException.ThrowIfNull(httpContextAccessor);

            ArgumentNullException.ThrowIfNull(factory);

            Instance = this;
            _httpContextAccessor = httpContextAccessor;
            _factory = factory;
            t = new Timer(Tick, UserStates, 60000, 60000);
            Task.Run(async () =>
            {
                using var context = await factory.CreateDbContextAsync();
                Timeout = context.AuthenticationSettings.FirstOrDefault()?.SessionTimeout;
            });
        }
        ~ApplicationUserStateService()
        {
            t?.Dispose();
        }

        /// <summary>
        /// Periodically checks for and removes stale user states based on session timeout settings.
        /// </summary>
        /// <param name="state">The UserStates List object, passed by the Timer.</param>
        private void Tick(object? state)
        {
            try
            {
                if (state is List<IApplicationUserState> userStates)
                {
                    var temp = new List<IApplicationUserState>(userStates); // Iterate over a copy
                    var now = DateTime.UtcNow;
                    temp.ForEach(x =>
                    {
                        if (Timeout.HasValue && (now - x.LastAccessed).TotalMinutes > Timeout * 3)
                        {
                            userStates.Remove(x); // Remove from original list
                            OnUserStateRemoved?.Invoke(x); // Invoke event
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Warning(ex, "ApplicationUserStateService.Tick: Exception during stale user state cleanup task.");
            }
        }

        /// <summary>Gets the <see cref="IApplicationUserState"/> for the currently authenticated user, based on the HTTP context. Returns null if no user context is available or an error occurs.</summary>
        public IApplicationUserState? CurrentUserState
        {
            get
            {
                try
                {
                    if (_httpContextAccessor?.HttpContext?.User != null)
                    {
                        return GetUserState(_httpContextAccessor.HttpContext.User);
                    }
                }
                catch (Exception ex)
                {
                    Loggers.SystemLogger.Error(ex, "Unexpected error trying to retrieve current user state from httpContext");
                }
                // If no user context is available or an error occurs, return null
                return null;

            }
        }

        /// <summary>Gets the username of the current user, if available; otherwise, an empty string.</summary>
        public string CurrentUsername
        {
            get
            {
                try
                {
                    return CurrentUserState?.User?.Identity?.Name ?? ""; // Simplified
                }
                catch (Exception ex) // Catching potential exceptions from CurrentUserState or its properties
                {
                    Loggers.SystemLogger.Warning(ex, "ApplicationUserStateService.CurrentUsername_get: Exception accessing CurrentUserState properties.");
                    return "";
                }
            }
        }

        /// <summary>Retrieves or creates and caches an <see cref="IApplicationUserState"/> for the given <see cref="ClaimsPrincipal"/>. Updates LastAccessed time for existing states.</summary> 
        /// <param name="userClaim">The user's <see cref="ClaimsPrincipal"/>. If null, the method returns null.</param> 
        /// <returns>The cached or newly created <see cref="IApplicationUserState"/>, or null if userClaim is null.</returns>
        public IApplicationUserState? GetUserState(ClaimsPrincipal userClaim)
        {
            if (userClaim == null)
            {
                Loggers.SystemLogger.Debug("ApplicationUserStateService.GetUserState: userClaim parameter is null. Returning null.");
                return null;
            }

            IApplicationUserState? existingState;
            lock (_userStatesLock)
            {
                existingState = UserStates.FirstOrDefault(s => s.User.FindFirstValue(ClaimTypes.Sid) == userClaim.FindFirstValue(ClaimTypes.Sid)
                                                           && s.User.FindFirstValue(ClaimTypes.Actor) == userClaim.FindFirstValue(ClaimTypes.Actor));
            }

            if (existingState == null)
            {
                existingState = CreateUserState(userClaim);
                Loggers.SystemLogger.Information("ApplicationUserStateService.GetUserState: No existing ApplicationUserState found for SID {UserSid}, ActorSID {ActorSid}. Creating and caching new state.", userClaim.FindFirstValue(ClaimTypes.Sid) ?? "N/A", userClaim.FindFirstValue(ClaimTypes.Actor) ?? "N/A");
                AddUserState(existingState); // This will also invoke UserStateAdded
            }

            existingState.LastAccessed = DateTime.UtcNow;
            return existingState;
        }

        private void AddUserState(IApplicationUserState state)
        {
            lock (_userStatesLock)
            {
                UserStates.Add(state);
                ApplicationEvents.LoggedOnUserCountChanged.Invoke(UserStates.Count);
            }

            UserStateAdded?.Invoke(state); // Invoke event after adding
        }
        /// <summary>Stores an MFA request temporarily, associating an MFA token with a user state and return URL. Typically used during an MFA challenge flow.</summary> 
        /// <param name="mfaToken">The MFA token (e.g., Duo state).</param> 
        /// <param name="state">The user state associated with this MFA attempt.</param> 
        /// <param name="returnURL">The URL to return to after MFA completion.</param>
        public void SetMFAUserState(MfaType mfaType, string mfaToken, IApplicationUserState state, string returnURL = "/")
        {
            ClaimsPrincipal? clonedPrincipal = null;
            if (state?.User != null)
            {
                clonedPrincipal = state.User.Clone();
            }

            if (clonedPrincipal != null)
            {
                // Create a fresh ApplicationUserState with the cloned principal so the queued MFARequest keeps a stable principal
                var oldState = state;
                state = CreateUserState(clonedPrincipal);
                state.PermissionDelegates.AddRange(oldState.PermissionDelegates);
                state.PermissionMappings.AddRange(oldState.PermissionMappings);
            }
            Loggers.SystemLogger.Information("ApplicationUserStateService.SetMFAUserState: Adding MFA request to queue for UserGUID {UserGUID}, MFAToken (hash): {MFATokenHash}.", state?.User?.FindFirstValue(ClaimTypes.Sid) ?? "Unknown", mfaToken?.GetAppHashCode().ToString() ?? "N/A");
            MFARequest mfaRequest = new(mfaType, mfaToken, returnURL, state);
            lock (_mfaQueueLock)
            {
                _mfaLoginQueue.Add(mfaRequest);
            }
            Task.Delay(90000).ContinueWith((val) =>
            {
                lock (_mfaQueueLock)
                {
                    _mfaLoginQueue.Remove(mfaRequest);
                }
            });
            SetUserState(state); // Ensure state is managed if not already
        }

        /// <summary>Retrieves and removes an <see cref="MFARequest"/> from the queue based on the MFA token.</summary> 
        /// <param name="mfaToken">The MFA token to search for.</param> 
        /// <returns>The <see cref="MFARequest"/> if found; otherwise, null.</returns>
        public MFARequest? GetMFARequest(string mfaToken)
        {
            lock (_mfaQueueLock)
            {
                var request = _mfaLoginQueue.FirstOrDefault(q => q.MfaToken.Equals(mfaToken));
                if (request != null)
                {
                    _mfaLoginQueue.Remove(request);
                }

                return request;
            }
        }

        /// <summary>Adds or updates an <see cref="IApplicationUserState"/> in the cache. Typically called internally or upon login.</summary> 
        /// <param name="state">The user state to cache.</param>
        public void SetUserState(IApplicationUserState state)
        {

            if (state != null)
            {
                var stateExists = true;
                lock (_userStatesLock)
                {
                    stateExists = UserStates.Contains(state); // Check if it's already there before adding
                }
                if (!stateExists)
                {
                    AddUserState(state);
                }

            }
        }

        /// <summary>Removes a specific <see cref="IApplicationUserState"/> instance from the cache.</summary> 
        /// <param name="state">The user state to remove. If null, a warning is logged.</param>
        public void RemoveUserState(IApplicationUserState state)
        {
            if (state == null)
            {
                Loggers.SystemLogger.Warning("ApplicationUserStateService.RemoveUserState: 'state' parameter is null. Cannot remove.");
                return;
            }
            try
            {
                lock (_userStatesLock)
                {
                    if (UserStates.Contains(state)) // Check before removing
                    {
                        UserStates.Remove(state);
                        OnUserStateRemoved?.Invoke(state); // Invoke event
                    }
                }
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex, "Error trying to remove user state");
            }
        }

        /// <summary>Removes the <see cref="IApplicationUserState"/> associated with the given <see cref="ClaimsPrincipal"/> from the cache.</summary> 
        /// <param name="currentUser">The ClaimsPrincipal whose state should be removed.</param>
        public void RemoveUserState(ClaimsPrincipal currentUser) => RemoveUserState(GetUserState(currentUser));

        /// <summary>Creates a new <see cref="IApplicationUserState"/> instance for the given <see cref="ClaimsPrincipal"/>.</summary> 
        /// <param name="user">The user's ClaimsPrincipal.</param> 
        /// <returns>A new <see cref="IApplicationUserState"/> instance.</returns>
        public IApplicationUserState CreateUserState(ClaimsPrincipal user)
        {
            return new ApplicationUserState(_factory) { User = user };
        }
    }

    /// <summary>
    /// Provides extension methods for registering session-related services.
    /// </summary>
    public static class ApplicationUserStateServiceHelpers
    {
        /// <summary>
        /// Adds session services, including <see cref="IApplicationUserStateService"/> as a singleton and <see cref="ICurrentUserStateService"/> as scoped, to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddSessionServices(this IServiceCollection services)
        {
            services.AddSingleton<IApplicationUserStateService, ApplicationUserStateService>();
            services.AddScoped<ICurrentUserStateService, CurrentUserStateService>();
            return services;
        }
    }
}