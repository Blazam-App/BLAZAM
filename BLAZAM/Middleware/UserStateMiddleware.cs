using BLAZAM.Session.Interfaces;

namespace BLAZAM.Server.Middleware
{
    /// <summary>
    /// Captures the web browser's authentication cookie to populate the CurrentUserStateService
    /// </summary>
    public class UserStateMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Creates an instance of this middleware
        /// </summary>
        /// <param name="next"></param>
        public UserStateMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        /// <summary>
        /// Executes this middleware
        /// </summary>
        /// <param name="httpContext">The browser context of the request</param>
        /// <param name="currentUserStateService">The current user service</param>
        /// <param name="userStateService">The application user service</param>
        /// <returns>A Task indicating competion of processing</returns>
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
    /// <summary>
    /// Extenstions for the <see cref="UserStateMiddleware"/>
    /// </summary>
    public static class UserStateMiddlewareExtensions
    {
        /// <summary>
        /// Adds the <see cref="UserStateMiddleware"/> service into middlware services
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseUserStateMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<UserStateMiddleware>();
        }
    }
}
