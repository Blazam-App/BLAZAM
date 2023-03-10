using BLAZAM.Server.Background;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BLAZAM.Server.Middleware
{
    public class HttpsRedirectionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ConnMonitor _monitor;

        public HttpsRedirectionMiddleware(
            RequestDelegate next,
            ConnMonitor monitor)
        {
            _next = next;
            _monitor = monitor;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            bool forceHttps;
            // If the value is not cached, retrieve it from the database.

            forceHttps = _monitor.RedirectToHttps;





            // If the ForceHttps flag is set to true, redirect to HTTPS.
            if (forceHttps 
                && !context.Request.IsHttps)
            {
                string httpsUrl = "https://" + context.Request.Host + context.Request.Path;
                context.Response.Redirect(httpsUrl);
                return;
            }

            // If the ForceHttps flag is not set or if the request is already HTTPS, proceed with the request.
            await _next(context);
        }



    }

}
