using BLAZAM.Common.Data.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace BLAZAM.Server.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
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
