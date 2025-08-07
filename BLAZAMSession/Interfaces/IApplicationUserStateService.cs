using System.Collections.Generic; // Required for IList
using System.Security.Claims; // Required for ClaimsPrincipal

namespace BLAZAM.Session.Interfaces
{
    /// <summary>
    /// Defines a contract for a service that manages the lifecycle and retrieval of <see cref="IApplicationUserState"/> instances.
    /// </summary>
    public interface IApplicationUserStateService
    {
        /// <summary>
        /// Gets the username of the user associated with the current HTTP context, if available.
        /// </summary>
        string CurrentUsername { get; }

        /// <summary>
        /// Gets the <see cref="IApplicationUserState"/> for the user associated with the current HTTP context. May be null if no user is authenticated or context is unavailable.
        /// </summary>
        IApplicationUserState? CurrentUserState { get; }


        /// <summary>
        /// Gets a list of all currently cached <see cref="IApplicationUserState"/> instances.
        /// </summary>
        IList<IApplicationUserState> UserStates { get; }

        /// <summary>
        /// Creates a new <see cref="IApplicationUserState"/> instance for the given <see cref="ClaimsPrincipal"/>. This does not automatically cache the state.
        /// </summary>
        /// <param name="user">The user's ClaimsPrincipal.</param>
        /// <returns>A new <see cref="IApplicationUserState"/> instance.</returns>
        IApplicationUserState CreateUserState(ClaimsPrincipal user);

        /// <summary>
        /// Retrieves and removes an <see cref="MFARequest"/> from the queue based on the MFA token.
        /// </summary>
        /// <param name="mfaToken">The MFA token to search for.</param>
        /// <returns>The <see cref="MFARequest"/> if found; otherwise, null.</returns>
        MFARequest? GetMFARequest(string mfaToken);

        /// <summary>
        /// Retrieves an existing <see cref="IApplicationUserState"/> for the given ClaimsPrincipal, or creates and caches a new one if none exists.
        /// </summary>
        /// <param name="userClaim">The user's ClaimsPrincipal.</param>
        /// <returns>The cached or newly created <see cref="IApplicationUserState"/>, or null if userClaim is null.</returns>
        IApplicationUserState? GetUserState(ClaimsPrincipal userClaim);

        /// <summary>
        /// Removes a specific <see cref="IApplicationUserState"/> instance from the cache.
        /// </summary>
        /// <param name="state">The user state to remove.</param>
        void RemoveUserState(IApplicationUserState state);

        /// <summary>
        /// Removes the <see cref="IApplicationUserState"/> associated with the given <see cref="ClaimsPrincipal"/> from the cache.
        /// </summary>
        /// <param name="currentUser">The ClaimsPrincipal whose state should be removed.</param>
        void RemoveUserState(ClaimsPrincipal currentUser);

        /// <summary>
        /// Temporarily stores MFA-related user state during an MFA challenge.
        /// </summary>
        /// <param name="mfaToken">The MFA token (e.g., Duo state).</param>
        /// <param name="state">The user state associated with this MFA attempt.</param>
        /// <param name="returnURL">The URL to return to after MFA completion. Defaults to "/".</param>
        void SetMFAUserState(string mfaToken, IApplicationUserState state, string returnURL = "/");

        /// <summary>
        /// Adds or updates an <see cref="IApplicationUserState"/> in the cache.
        /// </summary>
        /// <param name="state">The user state to cache.</param>
        void SetUserState(IApplicationUserState state);
    }
}