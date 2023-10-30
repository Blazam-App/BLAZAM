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

        public Task Invoke(HttpContext httpContext,ICurrentUserStateService currentUserStateService,IApplicationUserStateService userStateService)
        {
            currentUserStateService.State = userStateService.GetUserState(httpContext.User);
 
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
