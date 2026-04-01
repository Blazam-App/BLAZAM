using BLAZAM.Common.Data;
using BLAZAM.Database.Context;

namespace BLAZAM.Middleware
{
    /// <summary>
    /// Redirects the request to HTTPS is the
    /// request is HTTP if the database has
    /// force HTTPS set to true
    /// </summary>
    /// <remarks>
    /// Creates a new HTTPS redirect middleware to ensure users are using a secure connection
    /// </remarks>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="applicationInfo">Application information service.</param>
    /// <exception cref="ArgumentNullException">Thrown if applicationInfo is null.</exception>
    public class HttpsRedirectionMiddleware(
        RequestDelegate next,
        ApplicationInfo applicationInfo)
    {
        private readonly RequestDelegate _next = next;
        private readonly ApplicationInfo _applicationInfo = applicationInfo;

        /// <summary>
        /// Checks the database cache for a true ForceHTTPS and if so an request is HTTP redirect to HTTPS
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            bool forceHttps;
            // Check ForceHTTPS status from cached application settings.
            if (_applicationInfo.InstallationCompleted)
            {
                if (DatabaseCache.ApplicationSettings == null)
                {
                    Loggers.SystemLogger.Information("HttpsRedirectionMiddleware: DatabaseCache.ApplicationSettings is null. Assuming ForceHTTPS is false. This might be expected during initial setup or if settings are not yet loaded.");
                    forceHttps = false;
                }
                else
                {
                    forceHttps = DatabaseCache.ApplicationSettings.ForceHTTPS;
                }

                // If the ForceHttps flag is set to true, redirect to HTTPS.
                if (forceHttps
                    && !context.Request.IsHttps)
                {
                    string httpsUrl = "https://" + context.Request.Host + context.Request.Path;
                    Loggers.SystemLogger.Information("HttpsRedirectionMiddleware: Redirecting HTTP request {RequestPath} to HTTPS ({HttpsUrl}) based on ForceHTTPS setting.", context.Request.Path, httpsUrl);
                    context.Response.Redirect(httpsUrl);
                    return;
                }
            }
            // If the ForceHttps flag is not set or if the request is already HTTPS, proceed with the request.
            await _next(context);
        }
    }
}
