using BLAZAM.Common.Data;
using BLAZAM.Database.Context;
using BLAZAM.Pages.Error;

namespace BLAZAM.Server.Middleware
{
    /// <summary>
    /// Middleware to redirect requests based on the application's operational status (e.g., database connectivity, installation state).
    /// </summary>
    internal class ApplicationStatusRedirectMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ConnMonitor _monitor;
        private readonly List<string> _uriIgnoreList = new() { "/static", "/css", "/_content", "/_blazor", "/BLAZAM.styles.css", "/_framework" };
        private string intendedUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationStatusRedirectMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="monitor">The connection monitor service.</param>
        public ApplicationStatusRedirectMiddleware(
           RequestDelegate next,
           ConnMonitor monitor)
        {
            _next = next;
            _monitor = monitor;
        }

        /// <summary>
        /// Invokes the middleware to check application status and perform redirects if necessary.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <param name="factory">The database factory for user-specific contexts.</param>
        public async Task InvokeAsync(HttpContext context, IUserDatabaseFactory factory)
        {
            if (factory == null)
            {
                Loggers.SystemLogger.Error("IUserDatabaseFactory factory is null in ApplicationStatusRedirectMiddleware.InvokeAsync. Skipping status check.");
                await _next(context);
                return;
            }

            intendedUri = context.Request.Path.ToUriComponent();
            if (!InIgnoreList(intendedUri))
            {
                try
                {
                    switch (_monitor.AppReady)
                    {
                        case ServiceConnectionState.Connecting:
                            Loggers.SystemLogger.Information("ApplicationStatusRedirectMiddleware: App not ready (Connecting). Redirecting {intendedUri} to /.", intendedUri);
                            SendTo(context, "/");
                            break;
                        case ServiceConnectionState.Up:
                            var dbcontext = await factory.CreateDbContextAsync();
                            if (dbcontext.SeedMismatch)
                            {
                                RedirectToOops(context,
                                    "The application database is incompatible with this version of the application",
                                    "The database seed is different from the current version of the application",
                                    "Either install an older version of the application. Or create a new database to use with the new version.",
                                    "Database seed mismatch");
                            }
                            break;
                        case ServiceConnectionState.Down:
                            RedirectToOops(context,
                                "The application database is not reachable",
                                "The application failed to connect to the database.",
                                "Please check the connection string and ensure the database server is running.",
                                "Application database not reachable");
                            break;
                    }


                }
                catch (Exception ex)
                {
                    Loggers.SystemLogger.Error(ex, "Exception in ApplicationStatusRedirectMiddleware for intended URI: {intendedUri}", intendedUri);
                    SendTo(context, "/oops");

                }
            }
            await _next(context);
        }

        /// <summary>
        /// Redirects the response to the specified URI if it's different from the currently intended URI.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <param name="uri">The target URI for redirection.</param>
        private void SendTo(HttpContext context, string uri)
        {
            if (intendedUri != uri)
                context.Response.Redirect(uri);
        }

        /// <summary>
        /// Sets the Oops page messages and redirects the user to the /oops page.
        /// </summary>
        /// <param name="context">The current HttpContext.</param>
        /// <param name="errorMessage">The main error message to display.</param>
        /// <param name="detailsMessage">The detailed explanation of the error.</param>
        /// <param name="helpMessage">Helpful text for the user to resolve the error.</param>
        /// <param name="logReason">The reason for the redirect, for logging purposes.</param>
        private void RedirectToOops(HttpContext context, string errorMessage, string detailsMessage, string helpMessage, string logReason)
        {
            Oops.ErrorMessage = errorMessage;
            Oops.DetailsMessage = detailsMessage;
            Oops.HelpMessage = helpMessage;
            Loggers.SystemLogger.Warning("ApplicationStatusRedirectMiddleware: {logReason}. Redirecting {intendedUri} to /oops.", logReason, intendedUri);
            SendTo(context, "/oops");
        }

        /// <summary>
        /// Checks if the given URI path starts with any of the predefined ignored paths.
        /// </summary>
        /// <param name="uriPath">The URI path to check.</param>
        /// <returns>True if the path is in the ignore list, false otherwise.</returns>
        private bool InIgnoreList(string uriPath)
        {
            if (_uriIgnoreList.Any(x => uriPath.StartsWith(x))) return true;
            return false;
        }
    }
}