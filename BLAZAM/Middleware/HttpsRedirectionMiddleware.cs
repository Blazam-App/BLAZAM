using BLAZAM.Common.Data;
using BLAZAM.Database.Context;
using BLAZAM.Services.Background;

namespace BLAZAM.Server.Middleware
{
    public class HttpsRedirectionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ConnMonitor _monitor;
        private readonly ApplicationInfo _applicationInfo;

        public HttpsRedirectionMiddleware(
            RequestDelegate next,
            ConnMonitor monitor,
            ApplicationInfo applicationInfo)
        {
            _next = next;
            _monitor = monitor;
            _applicationInfo = applicationInfo;
        }

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
