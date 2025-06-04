// Import necessary namespaces for various functionalities
using BLAZAM.ActiveDirectory.Services; // Active Directory related services
using BLAZAM.Common.Attributes; // Custom attributes like AutoStartBackgroundService
using BLAZAM.Common.Conventions; // Custom routing conventions
using BLAZAM.Common.Data; // Common data structures and utilities
using BLAZAM.Common.Data.Services; // Common data services
using BLAZAM.Common.Exceptions; // Custom exception types
using BLAZAM.Database.Context; // Database context interfaces and implementations
using BLAZAM.Gui.Services; // GUI related services (MudBlazor extensions)
using BLAZAM.Notifications.Services; // Notification system services
using BLAZAM.Plugins; // Plugin system interfaces and helpers
using BLAZAM.Services; // Core application services
using BLAZAM.Services.Audit; // Auditing services
using BLAZAM.Services.Chat; // Chat services
using BLAZAM.Services.Duo; // Duo Security integration services
using BLAZAM.Session; // Session management services
using BLAZAM.Session.Interfaces; // Session management interfaces
using BLAZAM.Update.Services; // Application update services
using Microsoft.AspNetCore.Authentication; // ASP.NET Core Authentication services
using Microsoft.AspNetCore.Authentication.Cookies; // Cookie authentication scheme
using Microsoft.AspNetCore.Authentication.JwtBearer; // JWT Bearer authentication scheme
using Microsoft.IdentityModel.Tokens; // Security token handling
using Microsoft.OpenApi.Models; // OpenAPI (Swagger) models
using MudBlazor; // MudBlazor UI component library
using MudBlazor.Services; // MudBlazor service registration
using NuGet.Protocol.Plugins; // Used for PluginLoadContext (potentially, verify usage)
using Polly; // Resilience and transient-fault-handling library
using Polly.Contrib.WaitAndRetry; // Polly extensions for wait and retry strategies
using Polly.Extensions.Http; // Polly extensions for HttpClient
using System.Diagnostics; // For Process class
using System.Globalization; // For CultureInfo (localization)
using System.Management; // For WMI (Windows Management Instrumentation) to get Installation ID
using System.Reflection; // For assembly loading and reflection

namespace BLAZAM.Server
{
    /// <summary>
    /// Provides extension methods for <see cref="WebApplicationBuilder"/> and <see cref="WebApplication"/>
    /// to encapsulate application initialization, service injection, and pre-run configuration logic.
    /// </summary>
    public static class ProgramHelpers
    {
        /// <summary>
        /// Initializes core application properties like debug flags, installation ID,
        /// running process information, and application version.
        /// </summary>
        /// <param name="builder">The WebApplicationBuilder instance.</param>
        /// <returns>The modified WebApplicationBuilder instance.</returns>
        /// <remarks>Note the typo in the original method name 'IntializeProperties'.</remarks>
        public static WebApplicationBuilder IntializeProperties(this WebApplicationBuilder builder)
        {
            // Set a default timeout for Regex matching across the application domain.
            // Helps prevent potential ReDoS (Regular Expression Denial of Service) attacks.
            AppDomain.CurrentDomain.SetData("REGEX_DEFAULT_MATCH_TIMEOUT", TimeSpan.FromMilliseconds(100));

            // Initialize ApplicationInfo singleton (holds global app state/config).
            // Pass the builder to access configuration early.
            ApplicationInfo ApplicationInfo = new(builder);

            // Read DebugMode and DemoMode flags from configuration (appsettings.json).
            ApplicationInfo.inDebugMode = builder.Configuration.GetValue<bool>("DebugMode");
            ApplicationInfo.inDemoMode = builder.Configuration.GetValue<bool>("DemoMode");

            // Attempt to retrieve a unique installation ID for this machine.
            try
            {
                // Try getting the Windows Installation UUID via WMI.
                ApplicationInfo.installationId = GetInstallationId();
            }
            catch (Exception ex) // Catch broad exceptions as WMI can fail for various reasons (permissions, OS)
            {
                // Log the failure to get the preferred ID.
                Console.WriteLine($"Failed to get Windows Installation ID via WMI: {ex.Message}. Falling back to MachineName hash.");
                // Fallback: Generate a GUID based on the machine name. Less unique but better than nothing.
                ApplicationInfo.installationId = Environment.MachineName.ToGuid(); // Assumes ToGuid() extension method exists
            }

            // Define the path for application plugins based on the writable path.
            Program.PluginDirectory = new SystemDirectory(Program.WritablePath + $"plugins{Path.DirectorySeparatorChar}");

            // Store the configuration manager instance globally for easy access (use with caution).
            Program.Configuration = builder.Configuration;

            // Capture the current running process information.
            ApplicationInfo.runningProcess = Process.GetCurrentProcess();

            // Determine the application version from the executing assembly.
            ApplicationInfo.runningVersion = new ApplicationVersion(Assembly.GetExecutingAssembly());

            // Return the builder for chaining.
            return builder;
        }

        /// <summary>
        /// Attempts to retrieve the Windows installation UUID using WMI.
        /// </summary>
        /// <returns>A unique GUID representing the Windows installation.</returns>
        /// <exception cref="AppException">Thrown if the UUID cannot be retrieved (e.g., permission issues, WMI errors, or UUID not found).</exception>
        private static Guid GetInstallationId()
        {
            try
            {
                string ComputerName = "localhost"; // Target the local machine
                ManagementScope Scope;
                // Connect to the root\CIMV2 namespace
                Scope = new ManagementScope(String.Format("\\\\{0}\\root\\CIMV2", ComputerName), null);
                Scope.Connect(); // Establish connection

                // Query for the UUID from the Win32_ComputerSystemProduct class
                ObjectQuery Query = new("SELECT UUID FROM Win32_ComputerSystemProduct");
                ManagementObjectSearcher Searcher = new(Scope, Query);

                // Iterate through the results (should typically be only one)
                foreach (ManagementObject WmiObject in Searcher.Get())
                {
                    try
                    {
                        // Attempt to parse the UUID string into a Guid
                        return Guid.Parse(WmiObject["UUID"].ToString());
                    }
                    catch (Exception ex) // Catch parsing errors or null values
                    {
                        Console.WriteLine($"Error parsing UUID from WMI object: {ex.Message}. Skipping object.");
                        continue; // Try next object if any (though unlikely for this specific query)
                    }
                }
                // If the loop completes without returning, the UUID was not found.
                throw new AppException("WMI query executed successfully, but no Win32_ComputerSystemProduct UUID was found.");
            }
            catch (ManagementException ex) // Catch specific WMI errors
            {
                Console.WriteLine($"WMI ManagementException while getting Installation ID: {ex.Message}. Check WMI service and permissions.");
                throw new AppException("Failed to query WMI for Installation ID. Check WMI service status and application permissions.", ex);
            }
            catch (Exception ex) // Catch other potential errors (e.g., connection issues)
            {
                Console.WriteLine($"Generic exception while getting Installation ID: {ex.Message}");
                // Re-throw as AppException for consistent error handling upstream
                throw new AppException("An unexpected error occurred while retrieving the Installation ID.", ex);
            }
        }

        /// <summary>
        /// Registers application services with the dependency injection container.
        /// </summary>
        /// <param name="builder">The WebApplicationBuilder instance.</param>
        /// <returns>The modified WebApplicationBuilder instance.</returns>
        public static WebApplicationBuilder InjectServices(this WebApplicationBuilder builder)
        {
            // --- Localization Setup ---
            builder.Services.AddLocalization(); // Add localization services
            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                // Define supported cultures for the application
                var supportedCultures = new[]
                {
                    new CultureInfo("en-US"), // English (United States) - Often the default
                    new CultureInfo("fr-FR"), // French (France)
                    new CultureInfo("de"),    // German (Default)
                    new CultureInfo("es"),    // Spanish (Default)
                    new CultureInfo("hi"),    // Hindi
                    new CultureInfo("it"),    // Italian
                    new CultureInfo("ja"),    // Japanese
                    new CultureInfo("ko"),    // Korean
                    new CultureInfo("pl"),    // Polish
                    new CultureInfo("ru"),    // Russian
                    new CultureInfo("zh-Hans") // Chinese (Simplified)
                 };

                // Set the supported cultures for request processing and UI rendering
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
                // Could set DefaultRequestCulture here if needed
            });

            /* --- Code to force a specific culture during development/testing ---
            // Uncomment this block to force a specific culture for debugging localization.
            CultureInfo culture = new CultureInfo("zh-Hans"); // Example: Force Simplified Chinese
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            */

            // Register ApplicationInfo as a singleton service
            builder.Services.AddSingleton<ApplicationInfo>();

            // --- Authentication and Authorization Setup ---
            // Configure cookie policy (e.g., for GDPR consent)
            builder.Services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true; // Indicate that consent is needed
                options.MinimumSameSitePolicy = SameSiteMode.None; // Configure SameSite policy (adjust as needed for security)
            });

            // Add authentication services
            builder.Services.AddAuthentication(options =>
            {
                // Set default schemes for different authentication actions
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            // Add Cookie Authentication handler with specific options applied
            .AddCookie(AppAuthenticationStateProvider.ApplyAuthenticationCookieOptions()) // Assumes static method defines cookie options
            // Add JWT Bearer Authentication handler for API tokens
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true, // **Crucial**: Ensure the token signature is valid
                    // Use the symmetric key defined in the Encryption service
                    IssuerSigningKey = new SymmetricSecurityKey(Encryption.Instance.APITokenKey), // Assumes Encryption.Instance is available and key is set
                    ValidateIssuer = false, // Issuer validation disabled (can enable if needed)
                    ValidateAudience = false, // Audience validation disabled (can enable if needed)
                    ValidateActor = false, // Actor validation disabled
                    ValidateLifetime = true, // Ensure the token is not expired
                    // ClockSkew = TimeSpan.Zero // Optional: Adjust tolerance for time differences
                };
                // Register custom event handler for JWT authentication events (e.g., token validation)
                options.Events = new JwtAuthenticationEventsHandler(
                    // Resolve dependencies needed by the handler directly (using BuildServiceProvider here is generally discouraged, consider factory pattern)
                    builder.Services.BuildServiceProvider().GetRequiredService<IApplicationUserStateService>(),
                    builder.Services.BuildServiceProvider().GetRequiredService<IAppDatabaseFactory>()
                );
            });

            // Configure global authentication options
            builder.Services.Configure<AuthenticationOptions>(options =>
            {
                // If true, requires authentication even for sign-in pages (usually false)
                options.RequireAuthenticatedSignIn = false;
            });

            // --- Session Management ---
            builder.Services.AddDistributedMemoryCache(); // Add default in-memory distributed cache for session state
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(10); // Set session idle timeout (short duration, adjust as needed)
                options.Cookie.HttpOnly = true; // Prevent client-side script access to the session cookie
                options.Cookie.IsEssential = true; // Mark session cookie as essential (bypasses cookie consent policy)
            });

            // --- Blazor Setup ---
            builder.Services.AddRazorPages(); // Add support for Razor Pages
            builder.Services.AddServerSideBlazor() // Add Server-Side Blazor services
                .AddCircuitOptions(options =>
                {
                    // Show detailed error information in the browser console based on DebugMode flag
                    options.DetailedErrors = ApplicationInfo.inDebugMode;
                });

            // --- Database Context ---
            DatabaseContextBase.Configuration = builder.Configuration; // Provide configuration to the base context (static access, consider alternatives)
            // Register database context factories
            builder.Services.AddSingleton<IAppDatabaseFactory, AppDatabaseFactory>(); // Singleton factory for application-wide context
            builder.Services.AddScoped<IUserDatabaseFactory, UserDatabaseFactory>(); // Scoped factory for user-specific context (if needed)

            // --- HttpClient Configuration ---
            builder.Services.AddHttpClient(); // Register the basic IHttpClientFactory

            // Configure a named HttpClient for sending Webhooks with a retry policy
            builder.Services.AddHttpClient(HttpClientNames.WebHookHttpClientName) // Use a constant for the name
                   .SetHandlerLifetime(TimeSpan.FromMinutes(5)) // Define how long the HttpMessageHandler can be reused
                   .AddPolicyHandler(GetWebhookRetryPolicy()); // Add the Polly retry policy

            // Configure another named HttpClient for Webhooks that ignores SSL certificate errors
            builder.Services.AddHttpClient(HttpClientNames.WebHookHttpClientNoSSLCheckName)
                   .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                   .AddPolicyHandler(GetWebhookRetryPolicy())
                   .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                   {
                       // **Security Warning**: Bypassing SSL validation is insecure. Use only for trusted internal services or testing.
                       ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
                   });

            // --- Core Application Services ---
            builder.Services.AddHttpContextAccessor(); // Provides access to the current HttpContext (needed for user identity, etc.)
            builder.Services.AddSingleton<EmailService>(); // Service for sending emails
            builder.Services.AddSingleton<IChatService, ChatService>(); // Chat functionality service
            builder.Services.AddActiveDirectoryServices(); // Extension method to register AD-related services
            builder.Services.AddSingleton<ApplicationManager>(); // Core application management service
            builder.Services.AddScoped<PermissionApplicator>(); // Service to apply user permissions
            builder.Services.AddScoped<WebUserAuditLogger>(); // Scoped logger for web user actions
            builder.Services.AddSingleton<ServerAuditLogger>(); // Singleton logger for server-side events
            builder.Services.AddScoped<GoogleAuthenticatorService>(); // Service for Google Authenticator MFA
            builder.Services.AddScoped<JwtTokenService>(); // Service for creating and managing JWTs
            builder.Services.AddSingleton<WebHookPublisher>(); // Service for publishing webhook events
            builder.Services.AddScoped<AppAuthenticationStateProvider>(); // Custom Blazor authentication state provider
            builder.Services.AddScoped<SearchService>(); // Application-wide search functionality
            builder.Services.AddScoped<WidgetService>(); // Service to manage dashboard widgets based on permissions
            builder.Services.AddSingleton<IDuoClientProvider, DuoClientProvider>(); // Duo Security MFA integration
            // Register Encryption service as singleton (no benefit from multiple instances)
            builder.Services.AddSingleton<IEncryptionService, EncryptionService>();
            // Register connection monitoring service as singleton (runs a background timer)
            builder.Services.AddSingleton<ConnMonitor>();
            // Register notification publishing service as singleton
            builder.Services.AddSingleton<INotificationPublisher, NotificationPublisher>();

            // --- Background Services & Session Services ---
            builder.InjectBackgroundServices(); // Extension method to register background services automatically
            builder.Services.AddSessionServices(); // Extension method to register session-related services

            // --- Update Services ---
            builder.Services.AddUpdateServices(); // Extension method to register application update services

            // --- UI Services (MudBlazor) ---
            builder.Services.AddMudServices(configuration => // Add MudBlazor core services
            {
                // Configure Snackbar appearance and behavior
                configuration.SnackbarConfiguration.HideTransitionDuration = 250;
                configuration.SnackbarConfiguration.ShowTransitionDuration = 250;
                // Add other MudBlazor configurations here if needed (e.g., Position, MaxDisplayedSnackbars)
            });
            builder.Services.AddMudMarkdownServices(); // Add services for rendering Markdown using MudBlazor components
            builder.Services.AddScoped<AppSnackBarService>(); // Custom wrapper/service for MudBlazor Snackbar
            builder.Services.AddScoped<AppDialogService>(); // Custom wrapper/service for MudBlazor Dialog

            // --- Notification Generation ---
            builder.Services.AddSingleton<NotificationGenerationService>(); // Service responsible for generating notifications

            // --- MVC & API Controllers ---
            builder.Services.AddControllers(options =>
            {
                // Apply a custom convention to make controller routes lowercase
                options.Conventions.Add(new LowercaseControllerRouteConvention());
            });
            builder.Services.AddMvc(); // Add MVC services (includes controllers, views, etc.)

            // --- Swagger/OpenAPI Documentation ---
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo // Define API document information
                {
                    Title = "Blazam API",
                    Version = "v1",
                    Description = "The official Blazam API documentation." +
                                  "<br/>Authorization is required for API access." +
                                  "<br/>The \"Authorization\" header value must be \"Bearer {token}\"",
                    License = new OpenApiLicense() { Name = "MIT License", Url = new Uri("https://github.com/Blazam-App/BLAZAM/blob/v1-Dev/LICENSE") },
                    Contact = new()
                    {
                        Email = "support@blazam.org",
                        Name = "Blazam Support",
                        Url = new("https://blazam.org/support")
                    },
                    TermsOfService = new Uri("https://blazam.org/tos")
                });

                // Include XML comments from the assembly for richer descriptions (ensure XML doc generation is enabled in build settings)
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

                // Configure Swagger UI to use JWT Bearer authentication
                var jwtSecurityScheme = new OpenApiSecurityScheme
                {
                    Description = "Enter only the token supplied by Blazam (without 'Bearer ' prefix)", // User instruction
                    Name = "Authorization", // Header name
                    In = ParameterLocation.Header, // Location of the token
                    Type = SecuritySchemeType.Http, // Type of scheme
                    Scheme = JwtBearerDefaults.AuthenticationScheme, // Authentication scheme name ("Bearer")
                    BearerFormat = "JWT", // Format hint
                    Reference = new OpenApiReference // Reference for linking security requirements
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };

                // Add the security definition to Swagger
                c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
                // Add a security requirement globally (forces auth for all endpoints shown in Swagger UI)
                c.AddSecurityRequirement(new OpenApiSecurityRequirement() {
                    { jwtSecurityScheme, Array.Empty<string>() } // Link the requirement to the definition
                });
            });

            // --- Windows Service Hosting ---
            // Configure the application host to run as a Windows Service if applicable
            builder.Host.UseWindowsService();

            // --- Plugin Service Injection ---
            // Discover and inject services defined within plugins
            InjectPluginServices(builder);

            // Return the builder for chaining
            return builder;
        }

        /// <summary>
        /// Discovers plugins in the plugin directory, loads their assemblies,
        /// and calls their InjectServices method to register plugin-specific services.
        /// </summary>
        /// <param name="builder">The WebApplicationBuilder instance.</param>
        private static void InjectPluginServices(WebApplicationBuilder builder)
        {
            var pluginDir = Program.PluginDirectory; // Get the plugin directory path

            // Check if the directory exists
            if (!pluginDir.Exists)
            {
                Loggers.SystemLogger.Warning("Plugin directory {@PluginPath} does not exist. Skipping plugin loading.", pluginDir.FullPath);
                return;
            }

            Loggers.SystemLogger.Information("Scanning for plugins in {@PluginPath}...", pluginDir.FullPath);

            // Process each DLL file in the plugin directory in parallel
            Parallel.ForEach(pluginDir.Files.Where(f => f.Extension.Equals(".dll", StringComparison.OrdinalIgnoreCase)), dll =>
            {
                Loggers.SystemLogger.Debug("Attempting to load plugin assembly: {@DllName}", dll.Name);
                try
                {
                    // Use a custom AssemblyLoadContext for plugin isolation (optional but recommended)
                    var loadContext = new PluginLoadContext(dll.FullPath);
                    // Load the assembly by its name (without extension)
                    Assembly assembly = loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(dll.FullPath)));
                    Loggers.SystemLogger.Debug("Successfully loaded assembly: {@AssemblyName}", assembly.FullName);

                    // Find all types in the loaded assembly that implement IPluginBase
                    var pluginTypes = assembly.GetTypes()
                        .Where(type => typeof(IPluginBase).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);

                    // Iterate through discovered plugin types
                    foreach (Type pluginType in pluginTypes)
                    {
                        Loggers.SystemLogger.Information("Found plugin type: {@PluginType} in {@DllName}", pluginType.FullName, dll.Name);
                        try
                        {
                            // Create an instance of the plugin type
                            if (Activator.CreateInstance(pluginType) is IPluginBase pluginInstance)
                            {
                                // Call the plugin's service injection method
                                pluginInstance.InjectServices(builder);
                                // Store the loaded assembly reference in the plugin instance
                                pluginInstance.Assembly = assembly;
                                // Add the plugin instance to the global list of loaded plugins
                                ApplicationInfo.loadedPlugins.Add(pluginInstance);
                                Loggers.SystemLogger.Information("Successfully instantiated and injected services for plugin: {@PluginType}", pluginType.FullName);
                            }
                            else
                            {
                                Loggers.SystemLogger.Warning("Could not create an instance of plugin type: {@PluginType} in {@DllName}.", pluginType.FullName, dll.Name);
                            }
                        }
                        catch (Exception ex)
                        {
                            Loggers.SystemLogger.Error(ex, "Error creating instance or injecting services for plugin {@PluginType} in {@DllName}", pluginType.FullName, dll.Name);
                        }
                    }
                }
                // Handle errors during assembly loading or type discovery
                catch (ReflectionTypeLoadException ex) // Catches errors when types within the assembly cannot be loaded
                {
                    Loggers.SystemLogger.Error(ex, "Error loading types from assembly {@DllName}", dll.Name);
                    if (ex.LoaderExceptions != null)
                    {
                        foreach (Exception loaderEx in ex.LoaderExceptions)
                        {
                            Loggers.SystemLogger.Error("- LoaderException: {@LoaderExceptionMessage}", loaderEx.Message);
                        }
                    }
                }
                catch (FileLoadException ex) // Catches errors related to loading the file itself
                {
                    Loggers.SystemLogger.Error(ex, "Error loading assembly file {@DllName}", dll.Name);
                }
                catch (BadImageFormatException ex) // Catches errors if the DLL is not a valid .NET assembly
                {
                    Loggers.SystemLogger.Error(ex, "Error loading assembly {@DllName}: Invalid assembly format", dll.Name);
                }
                catch (Exception ex) // Catch-all for other unexpected errors
                {
                    Loggers.SystemLogger.Error(ex, "An unexpected error occurred while processing plugin assembly {@DllName}", dll.Name);
                }
            });
            Loggers.SystemLogger.Information("Finished scanning for plugins.");
        }

        // Lock object for thread safety when modifying shared service collection in parallel
        private static readonly object _lock = new();

        /// <summary>
        /// Discovers and registers background services marked with the <see cref="AutoStartBackgroundService"/> attribute
        /// from all loaded BLAZAM assemblies.
        /// </summary>
        /// <param name="builder">The WebApplicationBuilder instance.</param>
        /// <returns>The modified WebApplicationBuilder instance.</returns>
        public static WebApplicationBuilder InjectBackgroundServices(this WebApplicationBuilder builder)
        {
            Loggers.SystemLogger.Information("Injecting auto-start background services...");
            // Process relevant assemblies in parallel
            Parallel.ForEach(blazamAssemblies, assembly => // Use helper property to get BLAZAM assemblies
            {
                try
                {
                    // Find types marked with the AutoStartBackgroundService attribute
                    var types = assembly.GetTypes()
                        .Where(t => t.IsClass && !t.IsAbstract
                        && t.GetCustomAttribute<AutoStartBackgroundService>() != null);

                    foreach (var type in types)
                    {
                        // Find a suitable interface for registration (prefer interface over concrete type)
                        // Exclude common interfaces like IDisposable and the attribute interface itself
                        var interfaceType = type.GetInterfaces()
                            .FirstOrDefault(i => i.GetCustomAttribute<AutoStartBackgroundService>() == null
                            && i.Name != "IDisposable"); // Simple exclusion, might need refinement

                        // Lock to ensure thread-safe addition to the service collection
                        lock (_lock)
                        {
                            if (interfaceType != null)
                            {
                                // Register as singleton using the interface
                                builder.Services.AddSingleton(interfaceType, type);
                                Loggers.SystemLogger.Debug("Registered background service {@ServiceType} as {@InterfaceType}", type.FullName, interfaceType.FullName);
                            }
                            else
                            {
                                // Register as singleton using the concrete type if no suitable interface found
                                builder.Services.AddSingleton(type);
                                Loggers.SystemLogger.Debug("Registered background service {@ServiceType} (concrete)", type.FullName);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Loggers.SystemLogger.Error(ex, "Error injecting background services from assembly {@AssemblyName}", assembly.FullName);
                }
            });
            Loggers.SystemLogger.Information("Finished injecting auto-start background services.");
            return builder;
        }

        /// <summary>
        /// Performs pre-run initialization tasks after the application is built but before it starts listening.
        /// This includes setting up Seq logging based on database configuration and preloading/starting singleton services.
        /// </summary>
        /// <param name="application">The built WebApplication instance.</param>
        public static void PreRun(this WebApplication application)
        {
            Loggers.SystemLogger.Information("Performing PreRun tasks...");
            // Configure Seq logging based on database setting
            try
            {
                // Create a scope to resolve scoped services like the database context
                using var scope = application.Services.CreateScope();
                var dbFactory = scope.ServiceProvider.GetRequiredService<IAppDatabaseFactory>();
                using var context = dbFactory.CreateDbContext();

                if (context != null)
                {
                    var appSettings = context.AppSettings.FirstOrDefault();
                    if (appSettings != null)
                    {
                        // Enable/disable sending logs to the central Seq server based on the setting
                        Loggers.SendToSeqServer = appSettings.SendLogsToDeveloper != false; // Default to true if null
                        Loggers.SystemLogger.Information("Seq logging to developer server set to: {SendToSeq}", Loggers.SendToSeqServer);
                        ApplicationInfo.installationCompleted = appSettings.InstallationCompleted;
                        Loggers.SystemLogger.Information("Installation completed status: {Status}", ApplicationInfo.installationCompleted);
                    }
                    else
                    {
                        Loggers.SystemLogger.Warning("AppSettings record not found in database. Cannot determine Seq logging preference or installation status.");
                        ApplicationInfo.installationCompleted = false; // Assume not completed if settings are missing
                    }
                }
                else
                {
                    Loggers.SystemLogger.Information("Could not create database context during PreRun. Cannot determine Seq logging preference or installation status.");
                    ApplicationInfo.installationCompleted = false; // Assume not completed if DB context fails
                }
            }
            catch (Exception ex)
            {
                // Log errors during database access but don't prevent startup
                Loggers.SystemLogger.Information(ex, "Error accessing database during PreRun to configure Seq logging/check installation status.");
                ApplicationInfo.installationCompleted = false; // Assume not completed on error
            }

            // Preload/start singleton services (like background tasks)
            PreloadServices(application);
            Loggers.SystemLogger.Information("Finished PreRun tasks.");
        }

        /// <summary>
        /// Defines a Polly retry policy for HTTP requests, specifically for webhooks.
        /// Uses a decorrelated jitter backoff strategy to handle transient failures.
        /// </summary>
        /// <returns>An asynchronous Polly policy for HttpResponseMessage.</returns>
        private static IAsyncPolicy<HttpResponseMessage> GetWebhookRetryPolicy()
        {
            // Configure a backoff strategy with jitter to avoid thundering herd issues
            var delay = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(1), retryCount: 5);

            // Build the policy
            return HttpPolicyExtensions
                .HandleTransientHttpError() // Handles common transient HTTP errors (5xx, 408)
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound) // Also retry on 404 Not Found
                .WaitAndRetryAsync(delay);
        }

        /// <summary>
        /// Helper property to get all loaded assemblies whose names contain "BLAZAM".
        /// Used to target specific assemblies for reflection tasks like service discovery.
        /// </summary>
        private static IEnumerable<Assembly> blazamAssemblies => AppDomain.CurrentDomain.GetAssemblies()
                                                                    .Where(a => a.FullName?.Contains("BLAZAM", StringComparison.OrdinalIgnoreCase) == true);

        /// <summary>
        /// Preloads and starts singleton services, particularly those marked for auto-start
        /// and other essential services like UpdateService and statistics polling.
        /// </summary>
        /// <param name="application">The built WebApplication instance.</param>
        private static void PreloadServices(WebApplication application)
        {
            Loggers.SystemLogger.Information("Preloading/Starting singleton services...");
            try
            {
                // Only start most background services if the initial installation/setup is marked as complete.
                if (ApplicationInfo.installationCompleted)
                {
                    Loggers.SystemLogger.Information("Installation complete. Starting background services...");
                    // Iterate through BLAZAM assemblies to find auto-start services
                    foreach (var assembly in blazamAssemblies)
                    {
                        try
                        {
                            // Find types marked with the attribute
                            var types = assembly.GetTypes()
                                .Where(t => t.IsClass && !t.IsAbstract
                                && t.GetCustomAttribute<AutoStartBackgroundService>() != null);

                            foreach (var type in types)
                            {
                                try
                                {
                                    // Resolve the service instance from the DI container
                                    // Prefer resolving via interface if registered that way
                                    var interfaceType = type.GetInterfaces()
                                        .FirstOrDefault(i => i.GetCustomAttribute<AutoStartBackgroundService>() == null && i.Name != "IDisposable");

                                    BackgroundServiceBase? service = null;
                                    object? resolvedService = null;

                                    if (interfaceType != null)
                                    {
                                        resolvedService = application.Services.GetService(interfaceType); // Use GetService to avoid exception if not found
                                    }
                                    else
                                    {
                                        resolvedService = application.Services.GetService(type);
                                    }

                                    // Cast to BackgroundServiceBase (assuming this is the base class)
                                    service = resolvedService as BackgroundServiceBase;

                                    if (service != null)
                                    {
                                        // Get the attribute metadata to check if immediate start is requested
                                        var metadata = type.GetCustomAttribute<AutoStartBackgroundService>();
                                        Loggers.SystemLogger.Information("Starting background service: {ServiceType} (Immediate: {ImmediateStart})", type.FullName, metadata?.Immediate == true);
                                        // Start the service (implementation likely handles actual background task execution)
                                        service.Start(metadata?.Immediate == true);
                                    }
                                    else
                                    {
                                        Loggers.SystemLogger.Warning("Could not resolve or cast background service type {ServiceType} (Interface: {InterfaceType}) during preload.", type.FullName, interfaceType?.FullName ?? "N/A");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Loggers.SystemLogger.Error(ex, "Error resolving or starting background service {ServiceType} from assembly {AssemblyName}", type.FullName, assembly.FullName);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Loggers.SystemLogger.Error(ex, "Error finding or processing background service types in assembly {AssemblyName}", assembly.FullName);
                        }
                    }
                    Loggers.SystemLogger.Information("Finished starting background services.");
                }
                else
                {
                    Loggers.SystemLogger.Warning("Installation not marked as complete. Skipping startup of most background services.");
                }
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex, "Critical error enumerating assemblies or starting background services during preload.");
            }

            // Explicitly initialize/preload other critical singleton services, regardless of installation status (or check individually if needed)
            // Use try-catch blocks for each service to prevent one failure from stopping others.

            try
            {
                // Resolve NotificationGenerationService to ensure it's created
                Loggers.SystemLogger.Debug("Preloading NotificationGenerationService...");
                _ = application.Services.GetRequiredService<NotificationGenerationService>();
                Loggers.SystemLogger.Debug("NotificationGenerationService preloaded.");
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex, "Error preloading NotificationGenerationService.");
            }

            try
            {
                // Initialize UpdateService only if installation is complete
                if (ApplicationInfo.installationCompleted)
                {
                    Loggers.SystemLogger.Debug("Initializing UpdateService...");
                    var updateService = application.Services.GetRequiredService<UpdateService>();
                    updateService.Initialize(); // Call initialization method
                    Loggers.SystemLogger.Debug("UpdateService initialized.");
                }
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex, "Error initializing UpdateService.");
            }

            try
            {
                // Resolve WebHookPublisher only if installation is complete
                if (ApplicationInfo.installationCompleted)
                {
                    Loggers.SystemLogger.Debug("Preloading WebHookPublisher...");
                    _ = application.Services.GetRequiredService<WebHookPublisher>();
                    Loggers.SystemLogger.Debug("WebHookPublisher preloaded.");
                }
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex, "Error preloading WebHookPublisher.");
            }

            try
            {
                // Start resource usage polling only if installation is complete
                if (ApplicationInfo.installationCompleted)
                {
                    Loggers.SystemLogger.Debug("Starting application statistics polling...");
                    ApplicationStatistics.Process = ApplicationInfo.runningProcess; // Assign the process
                    ApplicationStatistics.StartResourceUsagePolling(); // Start the polling timer
                    Loggers.SystemLogger.Debug("Application statistics polling started.");
                }
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex, "Error starting application statistics polling.");
            }
            Loggers.SystemLogger.Information("Finished preloading/starting singleton services.");
        }
    }
}
