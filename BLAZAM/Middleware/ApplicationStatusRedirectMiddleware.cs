using BLAZAM.Common.Data.Database;
using BLAZAM.Server.Background;
using Microsoft.EntityFrameworkCore;

namespace BLAZAM.Server.Middleware
{
    internal class ApplicationStatusRedirectMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ConnMonitor _monitor;

        public ApplicationStatusRedirectMiddleware(
           RequestDelegate next,
           ConnMonitor monitor)
        {
            _next = next;
            _monitor = monitor;
        }

        public async Task InvokeAsync(HttpContext context, IDbContextFactory<DatabaseContext> factory)
        {
            string intendedUri = context.Request.Path.ToUriComponent();
            
            try
            {
                if (Program.InstallationCompleted )
                {
                    if (_monitor.AppReady != ConnectionState.Up)
                    {


                        if (_monitor.AppReady == ConnectionState.Connecting && intendedUri != "/")
                        {
                            context.Response.Redirect("/");
                        }
                        //else if (_monitor.AppReady == ConnectionState.Down && intendedUri != "/oops")
                        //{
                        //    context.Response.Redirect("/oops");
                        //}



                    }
               

                }
                else if (intendedUri != "/install")
                    {
                       // context.Response.Redirect("/install");

                    }
                

            }
            catch
            {
                if (intendedUri != "/oops")
                    context.Response.Redirect("/oops");

            }

            await _next(context);
        }
    }
}