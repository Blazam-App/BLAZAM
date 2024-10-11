using BLAZAM.Server.Data.Services;
using System.Security.Claims;

namespace BLAZAM.Session.Interfaces
{
    public interface IApplicationUserStateService
    {
        string CurrentUsername { get; }
        IApplicationUserState? CurrentUserState { get; }
        AppEvent<IApplicationUserState> UserStateAdded { get; set; }
        IList<IApplicationUserState> UserStates { get; }


        IApplicationUserState CreateUserState(ClaimsPrincipal user);
        MFARequest? GetMFARequest(string state);
        IApplicationUserState? GetUserState(ClaimsPrincipal userClaim);

        // IApplicationUserState? GetUserState(ClaimsPrincipal userClaim);
        void RemoveUserState(IApplicationUserState state);
        void RemoveUserState(ClaimsPrincipal currentUser);
        void SetMFAUserState(string mfaToken, IApplicationUserState state, string returnURL);
        void SetUserState(IApplicationUserState state);
    }
}