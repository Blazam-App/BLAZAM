using BLAZAM.Common.Data;
using BLAZAM.Database.Context;
using BLAZAM.Services.Background;

namespace BLAZAM.Server.Middleware
{
    /// <summary>
    /// Redirects the request to HTTPS is the
    /// request is HTTP if the database has
    /// force HTTPS set to true
    /// </summary>
    public class HttpsRedirectionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ApplicationInfo _applicationInfo;
        /// <summary>
        /// Creates a new HTTPS redirect middleware to ensure users are using a secure connection
        /// </summary>
        /// <param name="next"></param>
        /// <param name="applicationInfo"></param>
        public HttpsRedirectionMiddleware(
            RequestDelegate next,
            ApplicationInfo applicationInfo)
        {
            _next = next;
            _applicationInfo = applicationInfo;
        }
        /// <summary>
        /// Checks the database cache for a true ForceHTTPS and if so an request is HTTP redirect to HTTPS
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            bool forceHttps;
            // If the value is not cached, retrieve it from the database.
            if (_applicationInfo.InstallationCompleted)
            {
                try
                {
                    forceHttps = DatabaseCache.ApplicationSettings.ForceHTTPS;
                }
                catch (NullReferenceException ex)
                {
                    Loggers.SystemLogger.Warning("Error while checking database cache for Force HTTPS {@Error}", ex);
                    forceHttps = false;
                }




                // If the ForceHttps flag is set to true, redirect to HTTPS.
                if (forceHttps
                    && !context.Request.IsHttps)
                {
                    string httpsUrl = "https://" + context.Request.Host + context.Request.Path;
                    context.Response.Redirect(httpsUrl);
                    return;
                }

            }
            // If the ForceHttps flag is not set or if the request is already HTTPS, proceed with the request.
            await _next(context);
        }



    }

}
