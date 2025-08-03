using BLAZAM.Logger;
using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Http;
using System; // Added
using System.Security.Claims; // Added
using System.Threading; // Added

namespace BLAZAM.Session
{
    /// <summary>
    /// Provides scoped access to the current user's <see cref="IApplicationUserState"/>. It attempts to retrieve the state upon initialization and can retry if initially unavailable.
    /// </summary>
    public class CurrentUserStateService : IDisposable, ICurrentUserStateService
    {
        private IHttpContextAccessor _httpContextAccessor { get; set; }
        private readonly IApplicationUserStateService _applicationUserStateService;
        private Timer? _retryTimer;
        private IApplicationUserState state;

        /// <summary>
        /// Gets or sets the current user's application state. This state is retrieved via <see cref="IApplicationUserStateService"/> based on the current HTTP context.
        /// </summary>
        public IApplicationUserState State
        {
            get => state;
            set => state = value;
        }

        /// <summary>
        /// Gets the username of the current user from the <see cref="State"/>, if available.
        /// </summary>
        public string Username => State?.Username; // Added null conditional for safety

        /// <summary>Initializes a new instance of the <see cref="CurrentUserStateService"/> class.</summary> 
        /// <param name="applicationUserStateService">The singleton service for managing all application user states.</param> 
        /// <param name="httpContextAccessor">Accessor for the current HTTP context.</param> 
        /// <exception cref="ArgumentNullException">Thrown if applicationUserStateService or httpContextAccessor is null.</exception>
        public CurrentUserStateService(IApplicationUserStateService applicationUserStateService, IHttpContextAccessor httpContextAccessor)
        {
            ArgumentNullException.ThrowIfNull(applicationUserStateService);
            
            ArgumentNullException.ThrowIfNull(httpContextAccessor);

           

            _httpContextAccessor = httpContextAccessor;
            _applicationUserStateService = applicationUserStateService;
            RetryGetCurrentUserState(); // Initial attempt
            if (State is null)
            {
                _retryTimer = new Timer(RetryGetCurrentUserState, null, 500, 500);
            }
        }

        /// <summary>Attempts to retrieve and set the current user's state. If the state is not immediately available (e.g., during initial request processing), a timer may be used to retry.</summary> 
        /// <param name="state">Optional state object for timer callbacks (not used).</param>
        private void RetryGetCurrentUserState(object? state = null)
        {
            Loggers.SystemLogger.Information("Attempting to get user state from HTTPContext {UserName}", _httpContextAccessor.HttpContext?.User?.Identity?.Name);
            try
            {
                State = _applicationUserStateService.GetUserState(_httpContextAccessor.HttpContext?.User);
                if (State != null && State.IsAuthenticated)
                {
                    State.IPAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(); // Added null conditional for HttpContext
                    Loggers.SystemLogger.Information("CurrentUserStateService.RetryGetCurrentUserState: Successfully populated UserState for SID {UserSid}. IPAddress: {IPAddress}", State.User?.FindFirstValue(ClaimTypes.Sid) ?? "N/A", State.IPAddress ?? "N/A");
                    _retryTimer?.Dispose();
                }
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex, "Error trying to get current user state");
                return;
            }
            // Original log below might be less useful now that we log success specifically. Kept for now.
            Loggers.SystemLogger.Information("State Null:{StateNull} Username:{StateUsername}", State == null, State?.Username);
        }

        /// <summary>
        /// Disposes the retry timer if it was created.
        /// </summary>
        public void Dispose()
        {
            if (_retryTimer != null)
            {
                _retryTimer.Dispose();
            }
        }
    }
}
