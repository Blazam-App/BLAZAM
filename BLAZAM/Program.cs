// Import necessary namespaces for various functionalities like data handling,
// database operations, server configuration, logging, security, etc.
using BLAZAM.ActiveDirectory;
using BLAZAM.Common.Data;
using BLAZAM.Common.Exceptions;
using BLAZAM.Database.Context;
using BLAZAM.Database.Models;
using BLAZAM.Server;
using BLAZAM.Server.Middleware;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting.WindowsServices; // Required for running as a Windows Service
using Serilog; // Logging library
using System.Diagnostics; // For checking if debugger is attached
using System.Net; // For IP address handling
using System.Security.Cryptography.X509Certificates; // For SSL certificate handling

namespace BLAZAM
{
    /// <summary>
    /// Main entry point class for the BLAZAM web application.
    /// Handles application initialization, configuration, service injection,
    /// request pipeline setup, and lifecycle management.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Provides a path to the application's writable directory.
        /// This is typically used for storing temporary files, logs, etc.
        /// </summary>
        /// <returns>
        /// A SystemDirectory object representing the path, e.g., C:\inetpub\blazam\writable\
        /// </returns>
        internal static SystemDirectory WritablePath => new(ApplicationInfo.tempDirectory + $"writable{Path.DirectorySeparatorChar}");

   
        /// <summary>
        /// A static reference to the current running ASP.NET Core WebApplication instance.
        /// Provides access to application services and configuration throughout the application.
        /// </summary>
        public static WebApplication AppInstance { get; private set; }

        /// <summary>
        /// A static reference to the application's configuration manager.
        /// Holds configuration settings loaded from appsettings.json and other sources.
        /// </summary>
        public static ConfigurationManager? Configuration { get; set; }

        /// <summary>
        /// Indicates whether the application process has write permissions to the <see cref="WritablePath"/>.
        /// Checked during startup to ensure necessary file operations can be performed.
        /// </summary>
        public static bool Writable { get; private set; }

        /// <summary>
        /// The main entry point method for the web application.
        /// Execution starts here when the application is launched.
        /// </summary>
        /// <param name="args">Command-line arguments passed to the application.</param>
        public static void Main(string[] args)
        {
            // Log the current working directory for debugging purposes.
            Console.WriteLine("Working Directory: " + Environment.CurrentDirectory);

            // Create the WebApplicationBuilder.
            // Configure it to run as a Windows Service if detected.
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions()
            {
                // Set the content root path correctly when running as a Windows Service.
                ContentRootPath = WindowsServiceHelpers.IsWindowsService() ? AppContext.BaseDirectory : default,
                Args = args // Pass command-line arguments
            });

            // Ensure the configuration (e.g., appsettings.json) was loaded successfully.
            if (builder.Configuration is null)
                throw new AppException("The appsettings.json configuration file was not loaded");

            // Initialize custom application properties (likely from ApplicationInfo class).
            builder.IntializeProperties(); // Note: Typo in original method name 'IntializeProperties'

            // Initialize the Encryption service with the key from configuration.
            // The instance is discarded (_) as its constructor handles the setup.
            _ = new Encryption(Configuration?.GetValue<string>("EncryptionKey"));

            // Assign installation-specific details for logging context.
            Loggers.InstallationId = ApplicationInfo.installationId.ToString();
            Loggers.InstallationType = ApplicationInfo.isUnderIIS ? "IIS" : "Service"; // Determine if running under IIS or as a standalone service
            Loggers.DatabaseType = Configuration.GetValue<string>("DatabaseType"); // Get DB type from config

            // Configure Seq logging server details.
            Loggers.SeqServerUri = "http://logs.blazam.org:5341"; // Centralized logging server URI
            if (Debugger.IsAttached) // Use a specific API key when debugging
            {
                Loggers.SeqAPIKey = "xE50e1ljqtgLzHcu8pYC"; // Development/Debug API Key
            }
            else // Use a different API key for production/release builds
            {
                Loggers.SeqAPIKey = "8TeLknA8XBk5ybamT5m9"; // Production API Key
            }

            // Setup local file logging and Seq logging using Serilog.
            Loggers.SetupLoggers(WritablePath + $"logs{Path.DirectorySeparatorChar}", ApplicationInfo.runningVersion.ToString());
            // Integrate Serilog with the ASP.NET Core host logging.
            builder.Host.UseSerilog(Log.Logger);

            // Log the application start event with the process name.
            Loggers.SystemLogger.Warning("Application Starting {@ProcessName}", ApplicationInfo.runningProcess.ProcessName);

            // Register application services with the dependency injection container.
            builder.InjectServices();

            // Add Cross-Origin Resource Sharing (CORS) services.
            builder.Services.AddCors();

            // Configure the Kestrel web server (ports, SSL, etc.) if not running under IIS or debugging.
            SetupKestrel(builder);

            // Build the WebApplication instance from the builder.
            // This finalizes the service configuration and application setup.
            AppInstance = builder.Build();

            // Make the application's service provider available globally (use with caution).
            ApplicationInfo.services = AppInstance.Services;

            // Perform pre-run tasks (e.g., database migrations, initial setup).
            AppInstance.PreRun();

            // Re-setup loggers - perhaps needed if PreRun modified paths or settings?
            Loggers.SetupLoggers(WritablePath + $"logs{Path.DirectorySeparatorChar}", ApplicationInfo.runningVersion.ToString());

            // Enable Serilog request logging to log details about incoming HTTP requests.
            AppInstance.UseSerilogRequestLogging(configureOptions => configureOptions.Logger = Loggers.RequestLogger);

            // Configure the HTTP request processing pipeline (middleware).
            // Order is important here.

            // Configure exception handling based on the environment.
            if (!IsDevelopment && !ApplicationInfo.inDebugMode)
            {
                // Use a generic error handler page for production.
                AppInstance.UseExceptionHandler("/Error");

                // Enable HTTP Strict Transport Security (HSTS) for production.
                // Forces browsers to use HTTPS.
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                AppInstance.UseHsts();
            }
            else // Development environment specific configuration
            {
                // Provide detailed developer exception page in development.
                AppInstance.Environment.EnvironmentName = "Development"; // Explicitly set environment name if needed
                AppInstance.UseDeveloperExceptionPage();
            }

            // Custom middleware to manage user state.
            AppInstance.UseMiddleware<UserStateMiddleware>();
            // Redirect HTTP requests to HTTPS.
            AppInstance.UseMiddleware<HttpsRedirectionMiddleware>();
            // Custom middleware to redirect based on application status (e.g., maintenance mode).
            AppInstance.UseMiddleware<ApplicationStatusRedirectMiddleware>();
            // Enable serving static files (like CSS, JS, images) from wwwroot.
            AppInstance.UseStaticFiles();
            // Enable routing to match incoming requests to endpoints.
            AppInstance.UseRouting();
            // Enable Swagger API documentation generation.
            AppInstance.UseSwagger();
            // Enable Swagger UI for browsing the API documentation.
            AppInstance.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("v1/swagger.json", "Blazam API V1"); // Configure the endpoint
            });
            // Secure the Swagger endpoint, requiring authorization.
            AppInstance.MapSwagger().RequireAuthorization();
            // Enable cookie policy handling.
            AppInstance.UseCookiePolicy();
            // Enable authentication middleware.
            AppInstance.UseAuthentication();
            // Enable authorization middleware.
            AppInstance.UseAuthorization();
            // Enable session state management.
            AppInstance.UseSession();
            // Map controller endpoints.
            AppInstance.MapControllers();
            // Map the Blazor Hub for SignalR communication.
            AppInstance.MapBlazorHub();
            // Define a fallback page for requests that don't match other endpoints (typically the Blazor host page).
            AppInstance.MapFallbackToPage("/_Host");

            // Start the web server and begin listening for requests.
            AppInstance.Start();

            // Retrieve and log the actual addresses the server is listening on.
            GetRunningWebServerConfiguration();

            // Block the main thread until the application is shut down (e.g., Ctrl+C or service stop).
            AppInstance.WaitForShutdown();


            SecureLdapConnector.ClearPool();

            // Log application shutdown event.
            Loggers.SystemLogger.Information("Application Shutting Down");
        }

        /// <summary>
        /// Configures the Kestrel web server options, including listening addresses, ports, and HTTPS settings.
        /// This setup is typically bypassed when running under IIS, as IIS handles port binding and SSL termination.
        /// </summary>
        /// <param name="builder">The WebApplicationBuilder instance.</param>
        private static void SetupKestrel(WebApplicationBuilder builder)
        {
            // Create a temporary DbContext instance to fetch SSL settings.
            // This factory pattern avoids injecting the full DbContextFactory early in the setup.
            var _programDbFactory = new AppDatabaseFactory(Configuration);
            using var kestrelContext = _programDbFactory.CreateDbContext();

            // Configure Kestrel only if not running under IIS and not actively debugging.
            // When debugging, default launch settings often handle Kestrel configuration.
            if (!ApplicationInfo.isUnderIIS && !Debugger.IsAttached)
            {
                // Get Kestrel configuration values from appsettings.json.
                var listeningAddress = Configuration.GetValue<string>("ListeningAddress"); // e.g., "*" or specific IP
                var httpPort = Configuration.GetValue<int>("HTTPPort"); // e.g., 80 or 5000
                var httpsPort = Configuration.GetValue<int>("HTTPSPort"); // e.g., 443 or 5001

                AppSettings? dbSettings = null;
                X509Certificate2? cert = null;

                // Try to load SSL certificate details from the database.
                try
                {
                    dbSettings = kestrelContext.AppSettings.FirstOrDefault();
                    if (dbSettings != null)
                    {
                        // Decrypt the stored SSL certificate data.
                        var certBytes = dbSettings.SSLCertificateCipher?.Decrypt<byte[]>();
                        if (certBytes != null)
                        {
                            // Load the certificate from the byte array.
                            cert = new X509Certificate2(certBytes);
                            Loggers.SystemLogger.Information("SSL Certificate {@Subject} loaded successfully.", cert.Subject);
                        }
                        else
                        {
                            Loggers.SystemLogger.Warning("SSL Certificate data in database is null or could not be decrypted.");
                        }
                    }
                    else
                    {
                        // Log a warning if AppSettings couldn't be retrieved (e.g., initial setup).
                        Loggers.SystemLogger.Warning("Unable to retrieve AppSettings from DB for SSL information. HTTPS may not be configured.");
                    }
                }
                catch (Exception ex)
                {
                    // Log any errors during certificate retrieval or loading.
                    Loggers.SystemLogger.Error(ex, "Error collecting or loading SSL information from database.");
                }

                // Configure Kestrel endpoints.
                builder.WebHost.UseKestrel(options =>
                {
                    // Check if Kestrel should listen on all network interfaces.
                    if (listeningAddress == "*")
                    {
                        // Listen for HTTP requests on any IP address on the configured port.
                        options.ListenAnyIP(httpPort);
                        Loggers.SystemLogger.Information("Kestrel listening for HTTP on Any IP: {@Port}", httpPort);

                        // Configure HTTPS endpoint if port is specified, settings exist, certificate loaded, and has a private key.
                        if (httpsPort != 0 && dbSettings != null && cert != null && cert.HasPrivateKey)
                        {
                            options.ListenAnyIP(httpsPort, configure =>
                            {
                                // Use the loaded certificate for HTTPS.
                                configure.UseHttps(options => options.ServerCertificate = cert);
                            });
                            Loggers.SystemLogger.Information("Kestrel listening for HTTPS on Any IP: {@Port}", httpsPort);
                        }
                        else if (httpsPort != 0)
                        {
                            Loggers.SystemLogger.Warning("HTTPS port {@Port} configured, but SSL certificate is missing, invalid, or lacks a private key. HTTPS endpoint NOT configured.", httpsPort);
                        }
                    }
                    else // Listen on a specific IP address.
                    {
                        try
                        {
                            var ip = IPAddress.Parse(listeningAddress);

                            // Listen for HTTP requests on the specific IP and port.
                            options.Listen(ip, httpPort);
                            Loggers.SystemLogger.Information("Kestrel listening for HTTP on {@IP}:{@Port}", ip, httpPort);

                            // Configure HTTPS endpoint similarly for the specific IP.
                            if (httpsPort != 0 && dbSettings != null && cert != null && cert.HasPrivateKey)
                            {
                                options.Listen(ip, httpsPort, configure =>
                                {
                                    configure.UseHttps(options => options.ServerCertificate = cert);
                                });
                                Loggers.SystemLogger.Information("Kestrel listening for HTTPS on {@IP}:{@Port}", ip, httpsPort);
                            }
                            else if (httpsPort != 0)
                            {
                                Loggers.SystemLogger.Warning("HTTPS port {@Port} configured for IP {@IP}, but SSL certificate is missing, invalid, or lacks a private key. HTTPS endpoint NOT configured.", httpsPort, ip);
                            }
                        }
                        catch (FormatException ex)
                        {
                            Loggers.SystemLogger.Error(ex, "Invalid IP address format '{@IPAddress}' specified in ListeningAddress configuration. Kestrel HTTP/HTTPS endpoints may not be configured correctly.", listeningAddress);
                        }
                    }
                });
            }
            else
            {
                // Log why Kestrel setup is skipped.
                if (ApplicationInfo.isUnderIIS)
                    Loggers.SystemLogger.Information("Skipping Kestrel endpoint configuration because the application appears to be running under IIS.");
                if (Debugger.IsAttached)
                    Loggers.SystemLogger.Information("Skipping Kestrel endpoint configuration because the debugger is attached (likely using launchSettings.json).");
            }
        }


        /// <summary>
        /// Retrieves the actual addresses the web server is listening on after startup.
        /// </summary>
        private static void GetRunningWebServerConfiguration()
        {
            // Get the IServer service which represents the running web server (Kestrel or IISHttpServer).
            var server = AppInstance.Services.GetService<IServer>();
            if (server != null)
            {
                // Get the feature that provides the server's listening addresses.
                var addressFeature = server.Features.Get<IServerAddressesFeature>();
                if (addressFeature != null)
                {
                    // Store and log each address the server is bound to.
                    foreach (var address in addressFeature.Addresses)
                    {
                        ApplicationInfo.listeningAddresses = ApplicationInfo.listeningAddresses.Append(address);
                        Loggers.SystemLogger.Information("Application host is listening on: {@Address}", address);
                    }
                }
                else
                {
                    Loggers.SystemLogger.Warning("Could not retrieve IServerAddressesFeature to determine listening addresses.");
                }
            }
            else
            {
                Loggers.SystemLogger.Warning("Could not retrieve IServer service.");
            }
        }

        /// <summary>
        /// A convenience property to check if the application is running in the Development environment.
        /// </summary>
        public static bool IsDevelopment
        {
            get
            {
                // Check the environment name on the WebApplication instance.
                return AppInstance.Environment.IsDevelopment();
            }
        }

        /// <summary>
        /// Gets or sets the directory path for storing application plugins.
        /// </summary>
        public static SystemDirectory PluginDirectory { get; internal set; }
    }
}
