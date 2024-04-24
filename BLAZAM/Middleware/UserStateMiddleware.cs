using BLAZAM.Common.Data.Services;
using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Org.BouncyCastle.Ocsp;
using System.Threading.Tasks;

namespace BLAZAM.Server.Middleware
{
    /// <summary>
    /// Captures the web browser's authentication cookie to populate the CurrentUserStateService
    /// </summary>
    public class UserStateMiddleware
    {
        private readonly RequestDelegate _next;

        public UserStateMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext httpContext, ICurrentUserStateService currentUserStateService, IApplicationUserStateService userStateService)
        {
            if (httpContext != null && httpContext.User != null && httpContext.User.Identity != null)
            {
                if (httpContext.User.Identity.Name != null)
                {

                }
                var state = userStateService.GetUserState(httpContext.User);
                if (state != null)
                {
                    currentUserStateService.State = state;

                }
                if (httpContext.Connection != null &&
                    httpContext.Connection.RemoteIpAddress != null &&
                    currentUserStateService.State != null &&
                    currentUserStateService.State.IPAddress != httpContext.Connection.RemoteIpAddress.ToString())
                {
                    currentUserStateService.State.IPAddress = httpContext.Connection.RemoteIpAddress.ToString();
                }

            }
            return _next(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class UserStateMiddlewareExtensions
    {
        public static IApplicationBuilder UseUserStateMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<UserStateMiddleware>();
        }
    }
}
