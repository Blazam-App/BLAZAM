using System.Security.Claims;

namespace BLAZAM.Common.Data.Services
{
    public interface IApplicationUserStateService
    {
        string CurrentUsername { get; }
        IApplicationUserState? CurrentUserState { get; }
         AppEvent<IApplicationUserState> UserStateAdded { get; set; }
         IList<IApplicationUserState> UserStates { get; }

        IApplicationUserState? GetUserState(ClaimsPrincipal userClaim);

        // IApplicationUserState? GetUserState(ClaimsPrincipal userClaim);
        void RemoveUserState(IApplicationUserState state);
        void RemoveUserState(ClaimsPrincipal currentUser);
        void SetUserState(IApplicationUserState state);
    }
}