using BLAZAM.Session.Interfaces;

namespace BLAZAM.Middleware
{
    /// <summary>
    /// Captures the web browser's authentication cookie to populate the CurrentUserStateService
    /// </summary>
    /// <remarks>
    /// Creates an instance of this middleware
    /// </remarks>
    /// <param name="next">The next middleware in the pipeline.</param>
    public class UserStateMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        /// <summary>
        /// Executes this middleware
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="currentUserStateService">The service for managing the current user's state in the scope.</param>
        /// <param name="userStateService">The service for retrieving and managing application user states.</param>
        /// <returns>A Task indicating competion of processing</returns>
        public Task Invoke(HttpContext httpContext, ICurrentUserStateService currentUserStateService, IApplicationUserStateService userStateService)
        {
            if (httpContext == null)
            {
                Loggers.SystemLogger.Error("UserStateMiddleware: httpContext is null.");
                throw new AppException("HttpContext is null.");
            }


            if (httpContext.User != null && httpContext.User.Identity != null) //Proceed if User and Identity are not null
            {


                var state = userStateService.GetUserState(httpContext.User);
                if (state != null)
                {
                    currentUserStateService.State = state;
                }

                if (httpContext.Connection != null
                    && httpContext.Connection.RemoteIpAddress != null
                    && currentUserStateService.State != null
                    && currentUserStateService.State.IPAddress != httpContext.Connection.RemoteIpAddress.ToString())
                {


                    currentUserStateService.State.IPAddress = httpContext.Connection.RemoteIpAddress.ToString();
                    if (httpContext.Request.Headers.TryGetValue("User-Agent", out var userAgent))
                    {
                        currentUserStateService.State.Browser = userAgent.ToString();
                    }

                }
            }

            return _next(httpContext);
        }
    }

    /// <summary>
    /// Extension methods for adding the <see cref="UserStateMiddleware"/> to the application pipeline.
    /// </summary>
    public static class UserStateMiddlewareExtensions
    {
        /// <summary>
        /// Adds the <see cref="UserStateMiddleware"/> service into middlware services
        /// </summary>
        /// <param name="builder">The application builder.</param>
        /// <returns>The application builder.</returns>
        public static IApplicationBuilder UseUserStateMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<UserStateMiddleware>();
        }
    }
}
