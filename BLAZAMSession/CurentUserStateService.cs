using BLAZAM.Common.Data.Services;
using BLAZAM.Logger;
using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BLAZAM.Server.Data.Services
{
    public class CurrentUserStateService : IDisposable, ICurrentUserStateService
    {
        private IHttpContextAccessor _httpContextAccessor { get; set; }

        private readonly IApplicationUserStateService _applicationUserStateService;

        private Timer? _retryTimer;
        private IApplicationUserState state;

        //private static Dictionary<string, IApplicationUserState> _userStateCache = new Dictionary<string, IApplicationUserState>();

        /// <summary>
        /// The current user's session state
        /// </summary>
        public IApplicationUserState State
        {
            get => state;
            set => state = value;
        }

        /// <summary>
        /// The current user's username
        /// </summary>
        public string Username => State.Username;

        public CurrentUserStateService(IApplicationUserStateService applicationUserStateService, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _applicationUserStateService = applicationUserStateService;
            RetryGetCurrentUserState();
            if (State is null)
            {
                _retryTimer = new Timer(RetryGetCurrentUserState, null, 500, 500);
                return;
            }

        }

        private void RetryGetCurrentUserState(object? state = null)
        {

            try
            {
                State = _applicationUserStateService.GetUserState(_httpContextAccessor.HttpContext?.User);
                if (State != null && State.IsAuthenticated)
                    State.IPAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString();
                _retryTimer?.Dispose();

            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error("Error trying to get current user state {@Error}", ex);
                return;
            }


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
