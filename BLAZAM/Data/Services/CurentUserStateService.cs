using BLAZAM.Common.Data.Services;
using System.Security.Claims;

namespace BLAZAM.Server.Data.Services
{
    public class CurrentUserStateService : IDisposable, ICurrentUserStateService
    {
        private readonly IApplicationUserStateService _applicationUserStateService;

        private Timer? _retryTimer;
        private IApplicationUserState state;



        /// <summary>
        /// The current user's session state
        /// </summary>
        public IApplicationUserState State { get => state; set => state = value; }

        /// <summary>
        /// The current user's username
        /// </summary>
        public string Username => State.Username;

        public CurrentUserStateService(IApplicationUserStateService applicationUserStateService)
        {
            _applicationUserStateService = applicationUserStateService;
            RetryGetCurrentUserState();
            if (State is null)
            {
                _retryTimer = new Timer(RetryGetCurrentUserState, null, 500, 500);
            }
        }

        private void RetryGetCurrentUserState(object? state = null)
        {
            var currentState = _applicationUserStateService.CurrentUserState;
            if (currentState != null)
            {
                State = currentState;
                _retryTimer?.Dispose();
            }

        }
        public IApplicationUserState? CreateUserState(ClaimsPrincipal user)
        {
            return _applicationUserStateService.CreateUserState(user);
        }
        public void Dispose()
        {
            if (_retryTimer != null)
            {
                _retryTimer.Dispose();
            }
        }
    }
}
