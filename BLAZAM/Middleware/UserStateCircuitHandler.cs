using Microsoft.AspNetCore.Components.Server.Circuits;
using BLAZAM.Session.Interfaces;

namespace BLAZAM.Middleware
{
    /// <summary>
    /// Handles user state updates for Blazor circuits when a new connection is established.
    /// </summary>
    /// <remarks>This handler updates user-specific information, such as IP address and browser details, when
    /// the user is authenticated. It is intended for use in Blazor Server applications to track user state during
    /// circuit lifecycle events. The handler completes synchronously and does not throw exceptions.</remarks>
    public class UserStateCircuitHandler : CircuitHandler
    {
        private readonly IApplicationUserStateService _userStateService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        /// <summary>
        /// Initializes a new instance of the UserStateCircuitHandler class with the specified user state service and
        /// HTTP context accessor.  
        /// </summary>
        /// <param name="userStateService">The service used to manage and retrieve application user state information.</param>
        /// <param name="httpContextAccessor">The accessor used to obtain the current HTTP context for user-related operations.</param>
        public UserStateCircuitHandler(
            IApplicationUserStateService userStateService,
            IHttpContextAccessor httpContextAccessor)
        {
            _userStateService = userStateService;
            _httpContextAccessor = httpContextAccessor;
        }
        /// <summary>
        /// Handles logic when a new connection is established for the specified circuit.
        /// </summary>
        /// <remarks>If the user is authenticated, updates the user state with the current IP address and
        /// browser information. This method does not throw exceptions and completes synchronously.</remarks>
        /// <param name="circuit">The circuit representing the user's connection to the application.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A completed task representing the asynchronous operation.</returns>
        public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                var state = _userStateService.GetUserState(httpContext.User);

                if (state != null)
                {
                    // Note: ICurrentUserStateService is scoped per circuit, 
                    // so we can't inject it in the constructor
                    // You'll need to handle this differently - see below
                    
                    if (httpContext.Connection?.RemoteIpAddress != null)
                    {
                        state.IPAddress = httpContext.Connection.RemoteIpAddress.ToString();
                    }

                    if (httpContext.Request.Headers.TryGetValue("User-Agent", out var userAgent))
                    {
                        state.Browser = userAgent.ToString();
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}