using BLAZAM.Common.Data;
using BLAZAM.Database.Context;
using BLAZAM.Pages.Error;

namespace BLAZAM.Server.Middleware
{
    internal class ApplicationStatusRedirectMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ConnMonitor _monitor;
        private readonly List<string> _uriIgnoreList = new() { "/static", "/css", "/_content", "/_blazor", "/BLAZAM.styles.css", "/_framework" };
        private string intendedUri;

        public ApplicationStatusRedirectMiddleware(
           RequestDelegate next,
           ConnMonitor monitor)
        {
            _next = next;
            _monitor = monitor;
        }

        public async Task InvokeAsync(HttpContext context, IUserDatabaseFactory factory)
        {
            intendedUri = context.Request.Path.ToUriComponent();
            if (!InIgnoreList(intendedUri))
            {
                try
                {
                    switch (_monitor.AppReady)
                    {
                        case ServiceConnectionState.Connecting:
                            SendTo(context, "/");
                            break;
                        case ServiceConnectionState.Up:
                            var dbcontext = await factory.CreateDbContextAsync();
                            if (dbcontext.SeedMismatch)
                            {
                                Oops.ErrorMessage = "The application database is incompatible with this version of the application";
                                Oops.DetailsMessage = "The database seed is different from the current version of the application";
                                Oops.HelpMessage = "Either install an older version of the application. Or create a new database to use with the new version.";
                                SendTo(context, "/oops");

                            }
                            break;
                        case ServiceConnectionState.Down:
                            //Oops.ErrorMessage = "The application database is not reachable";
                             //SendTo(context, "/oops");

                            break;
                    }


                }
                catch
                {
                    SendTo(context, "/oops");

                }
            }
            await _next(context);
        }

        //Sets the response header to redirect to
        private void SendTo(HttpContext context, string uri)
        {
            if (intendedUri != uri)
                context.Response.Redirect(uri);
        }

        private bool InIgnoreList(string intendedUri)
        {
            if (_uriIgnoreList.Any(x => intendedUri.StartsWith(x))) return true;
            return false;
        }
    }
}