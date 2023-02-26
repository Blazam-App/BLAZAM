using BLAZAM.Common.Data.Database;
using BLAZAM.Server.Background;
using Microsoft.EntityFrameworkCore;

namespace BLAZAM.Server.Middleware
{
    internal class ApplicationStatusRedirectMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ConnMonitor _monitor;
        private readonly List<string> _uriIgnoreList = new List<string> { "/css", "/_content","/_blazor","/BLAZAM.styles.css","/_framework" };
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
            if (!InIgnoreList(intendedUri))
            {
                try
                {
                    switch (_monitor.AppReady)
                    {
                        case ConnectionState.Connecting:
                            if (intendedUri != "/")
                            {
                                context.Response.Redirect("/");
                            }
                            break;
                        case ConnectionState.Up:
                            var dbContext = factory.CreateDbContext();
                            if (!Program.InstallationCompleted)
                            {
                                if (intendedUri != "/install")
                                {
                                    context.Response.Redirect("/install");

                                }
                            }
                            break;
                        case ConnectionState.Down:
                            if (intendedUri != "/oops")
                            {
                                context.Response.Redirect("/oops");
                            }
                            break;
                    }
                    /*
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
                    */

                }
                catch
                {
                    if (intendedUri != "/oops")
                        context.Response.Redirect("/oops");

                }
            }
            await _next(context);
        }

        private bool InIgnoreList(string intendedUri)
        {
            foreach(var uri in _uriIgnoreList) { 
                if (intendedUri.StartsWith(uri)) return true;
            }
            return false;
        }
    }
}