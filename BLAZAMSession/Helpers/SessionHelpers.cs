using BLAZAM.Database.Context;
using BLAZAM.Logger;
using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BLAZAM.Helpers
{
    /// <summary>
    /// Provides extension methods for HttpContext related to session management, particularly cookie expiration.
    /// </summary>
    public static class SessionHelpers
    {
        /// <summary>
        /// Checks if the application user state represents an 'admin' or 'demo' user based on username.
        /// </summary>
        /// <param name="state">The application user state.</param>
        /// <returns>True if the username is 'admin' or 'demo' (case-insensitive); otherwise, false. Returns false if state or username is null.</returns>
        public static bool IsAdminOrDemo(this IApplicationUserState state)
        {
            // state?.Username handles null state, ?.Equals handles null Username
            return state?.Username?.Equals("admin", StringComparison.InvariantCultureIgnoreCase) == true ||
                   state?.Username?.Equals("demo", StringComparison.InvariantCultureIgnoreCase) == true;
        }

        /// <summary>
        /// Attempts to slide the expiration of the ASP.NET Core authentication cookie based on the session timeout configured in application settings. Logs warnings on failure.
        /// </summary>
        /// <param name="httpContext">The current HttpContext.</param>
        /// <param name="userState">Optional. The current user's application state, used for logging context.</param>
        public static void SlideCookieExpiration(this HttpContext httpContext, IApplicationUserState? userState = null)
        {
            if (httpContext == null)
            {
                Loggers.SystemLogger.Warning("SessionHelpers.SlideCookieExpiration: httpContext parameter is null. Cannot slide cookie expiration.");
                return;
            }
            try
            {
                if (DatabaseCache.AuthenticationSettings?.SessionTimeout != null)
                {
                    var cookie = httpContext.Request.Cookies[CookieAuthenticationDefaults.CookiePrefix + CookieAuthenticationDefaults.AuthenticationScheme];
                    if (cookie != null)
                    {
                        var ticketDataFormat = httpContext.RequestServices.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>().Get(CookieAuthenticationDefaults.AuthenticationScheme).TicketDataFormat;
                        var ticket = ticketDataFormat.Unprotect(cookie);
                        if (ticket != null)
                        {
                            var currentUtc = DateTimeOffset.UtcNow;
                            var dbTimeoutValue = (double)DatabaseCache.AuthenticationSettings.SessionTimeout;
                            if (ticket.Properties.IssuedUtc.Value.AddMinutes(dbTimeoutValue) > currentUtc)
                            {
                                var newExpiryTime = currentUtc.AddMinutes(dbTimeoutValue);
                                Loggers.SystemLogger.Debug("SessionHelpers.SlideCookieExpiration: Sliding cookie expiration for user {UserIdentifier} to {NewExpiryTime}.", userState?.User?.Identity?.Name ?? httpContext.User?.Identity?.Name ?? "Unknown", newExpiryTime);

                                ticket.Properties.IssuedUtc = currentUtc;
                                ticket.Properties.ExpiresUtc = newExpiryTime;

                                var newCookie = ticketDataFormat.Protect(ticket);
                                httpContext.Response.Cookies.Append(
                                    CookieAuthenticationDefaults.CookiePrefix + CookieAuthenticationDefaults.AuthenticationScheme,
                                    newCookie,
                                    new CookieOptions
                                    {
                                        HttpOnly = true,
                                        Secure = true, // Assuming Secure attribute is desired
                                        Expires = ticket.Properties.ExpiresUtc
                                    });
                                if (userState != null)
                                    userState.Ticket = ticket;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Warning(ex, "SessionHelpers.SlideCookieExpiration: Failed to slide cookie expiration for user {UserIdentifier}.", userState?.User?.Identity?.Name ?? httpContext.User?.Identity?.Name ?? "Unknown");
            }
        }

        /// <summary>
        /// Retrieves the configured session timeout duration from the authentication cookie, if available. Logs warnings on failure.
        /// </summary>
        /// <param name="httpContext">The current HttpContext.</param>
        /// <returns>A TimeSpan representing the session timeout, or null if it cannot be determined or an error occurs.</returns>
        public static TimeSpan? GetSessionTimeout(this HttpContext httpContext)
        {
            if (httpContext == null)
            {
                Loggers.SystemLogger.Warning("SessionHelpers.SessionTimeout: httpContext parameter is null. Cannot retrieve session timeout.");
                return null;
            }
            try
            {
                string? cookie = httpContext.GetAuthenticationCookie();
                if (cookie != null)
                {
                    var ticketDataFormat = httpContext.RequestServices.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>().Get(CookieAuthenticationDefaults.AuthenticationScheme).TicketDataFormat;
                    var ticket = ticketDataFormat.Unprotect(cookie);
                    if (ticket != null && ticket.Properties.ExpiresUtc.HasValue && ticket.Properties.IssuedUtc.HasValue)
                    {
                        return ticket.Properties.ExpiresUtc.Value - ticket.Properties.IssuedUtc.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Warning(ex, "SessionHelpers.SessionTimeout: Failed to retrieve session timeout for user {UserIdentifier}.", httpContext.User?.Identity?.Name ?? "Unknown");
                return null;
            }
            return null;
        }

        /// <summary>
        /// Gets the ASP.NET Core authentication cookie value from the HttpContext.
        /// </summary>
        private static string? GetAuthenticationCookie(this HttpContext httpContext)
        {
            // Get the current authentication cookie
            return httpContext.Request.Cookies[CookieAuthenticationDefaults.CookiePrefix + CookieAuthenticationDefaults.AuthenticationScheme];
        }
    }
}
