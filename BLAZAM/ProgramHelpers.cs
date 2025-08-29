// Import necessary namespaces for various functionalities
using System.Diagnostics;
using System.Globalization;
using System.Management;
using System.Reflection;
using System.Text.Json.Serialization;
using BLAZAM.Common.Conventions;
using BLAZAM.Common.Data;
using BLAZAM.Common.Data.Services;
using BLAZAM.Data;
using BLAZAM.Database.Context;
using BLAZAM.Global.Attributes;
using BLAZAM.Global.Data.Strings;
using BLAZAM.Gui.Services;
using BLAZAM.Notifications.Services;
using BLAZAM.Plugins;
using BLAZAM.Services;
using BLAZAM.Services.Audit;
using BLAZAM.Services.Chat;
using BLAZAM.Services.Duo;
using BLAZAM.Session;
using BLAZAM.Session.Interfaces;
using BLAZAM.Update.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MudBlazor;
using MudBlazor.Services;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;

namespace BLAZAM
{
    /// <summary>
    /// Extension methods for <see cref="WebApplicationBuilder"/> and <see cref="WebApplication"/>
    /// to encapsulate application initialization, service injection, and pre-run configuration logic.
    /// </summary>
    public static class ProgramHelpers
    {
        /// <summary>
        /// Initializes core application properties like debug flags, installation ID,
        /// running process information, and application version.
        /// </summary>
        public static WebApplicationBuilder IntializeProperties(this WebApplicationBuilder builder)
        {
            // Set a default timeout for regex operations to prevent excessive processing time.
            AppDomain.CurrentDomain.SetData("REGEX_DEFAULT_MATCH_TIMEOUT", TimeSpan.FromMilliseconds(30000));

            // Initialize ApplicationInfo singleton (holds global app state/config).
            // Pass the builder to access configuration early.
            _ = new ApplicationInfo(builder);

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
                Loggers.SystemLogger.Information(ex, "Failed to get Windows Installation ID via WMI. Falling back to MachineName hash.");
                // Fallback: Generate a GUID based on the machine name. Less unique but better than nothing.
                ApplicationInfo.installationId = Environment.MachineName.ToGuid();
            }

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
                var scope = new ManagementScope(@"\\localhost\root\CIMV2", null);
                scope.Connect();

                var query = new ObjectQuery("SELECT UUID FROM Win32_ComputerSystemProduct");
                var searcher = new ManagementObjectSearcher(scope, query);


                try
                {
                    var enumerator = searcher.Get().GetEnumerator();
                    var wmiObject = enumerator.MoveNext() ? enumerator.Current : null;
                    if (wmiObject != null)
                    {
                        var guid = wmiObject["UUID"];
                        if (guid == null)
                        {
                            throw new AppException("UUID property not found in Win32_ComputerSystemProduct WMI object.");
                        }
                        var guidString = guid.ToString();
                        if (guidString == null)
                        {
                            throw new AppException("UUID property is null in Win32_ComputerSystemProduct WMI object.");
                        }
                        return Guid.Parse(guidString);
                    }
                }
                catch (Exception ex)
                {
                    Loggers.SystemLogger.Information(ex, "Error parsing UUID from WMI object. Skipping object.");

                }

                throw new AppException("WMI query executed successfully, but no Win32_ComputerSystemProduct UUID was found.");
            }
            catch (ManagementException ex)
            {
                Loggers.SystemLogger.Information(ex, "WMI ManagementException while getting Installation ID. Check WMI service and permissions.");
                throw new AppException("Failed to query WMI for Installation ID. Check WMI service status and application permissions.", ex);
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Information(ex, "Generic exception while getting Installation ID");
                throw new AppException("An unexpected error occurred while retrieving the Installation ID.", ex);
            }
        }

        /// <summary>
        /// Registers application services with the dependency injection container.
        /// </summary>
        public static WebApplicationBuilder InjectServices(this WebApplicationBuilder builder)
        {
            // --- Localization Setup ---
            builder.Services.AddLocalization(); // Add localization services
            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                // Define supported cultures for the application
                var supportedCultures = new[]
                {
                    new CultureInfo("ar"),    // Arabic
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
          
            CultureInfo culture = new CultureInfo("ar"); // Example: Force Simplified Chinese
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
                options.IdleTimeout = TimeSpan.FromSeconds(10); // Set session idle timeout (short duration, lengthened on cookie refresh)
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
#pragma warning disable S4830 // Server certificates should be verified during SSL/TLS connections
            builder.Services.AddHttpClient(HttpClientNames.WebHookHttpClientNoSSLCheckName)
                   .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                   .AddPolicyHandler(GetWebhookRetryPolicy())
                   .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                   {
                       // **Security Warning**: Bypassing SSL validation is insecure. Use only for trusted internal services or testing.
                       ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
                   });
#pragma warning restore S4830 // Server certificates should be verified during SSL/TLS connections

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
            builder.Services.AddScoped<Analytics>(); // Service for Google Analytics of user actions
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
            }).AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
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
                c.SchemaFilter<EnumSchemaFilter>();
                // Add the security definition to Swagger
                c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
                // Add a security requirement globally (forces auth for all endpoints shown in Swagger UI)
                c.AddSecurityRequirement(new OpenApiSecurityRequirement() {
                    { jwtSecurityScheme, Array.Empty<string>() } // Link the requirement to the definition
                });
            });



            // Add response compression services
            builder.Services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes = new[]
                {
                    "text/plain",
                    "text/css",
                    "application/javascript",
                    "text/javascript",
                    "application/json",
                    "application/xml",
                    "text/html",
                    "image/svg+xml"
                };
            });

            // --- Windows Service Hosting ---
            // Configure the application host to run as a Windows Service if applicable
            builder.Host.UseWindowsService();

            // --- Plugin Service Injection ---
            // Discover and inject services defined within plugins
            LoadPluginAssemblies(builder);

            // Return the builder for chaining
            return builder;
        }

        /// <summary>
        /// Discovers plugins in the plugin directory, loads their assemblies,
        /// and calls their InjectServices method to register plugin-specific services.
        /// </summary>
        private static void LoadPluginAssemblies(WebApplicationBuilder builder)
        {
            var pluginDir = ApplicationInfo.pluginDirectory;
            if (!pluginDir.Exists)
            {
                Loggers.PluginLogger.Warning("Plugin directory {@PluginPath} does not exist. Skipping plugin loading.", pluginDir.FullPath);
                return;
            }

            Loggers.PluginLogger.Information("Scanning for plugins in {@PluginPath}...", pluginDir.FullPath);
            Parallel.ForEach(pluginDir.GetFilesAndSubFiles("*.dll"), dll => LoadAndProcessPlugin(dll, builder));
            Loggers.PluginLogger.Information("Finished scanning for plugins.");
        }

        /// <summary>
        /// Loads a single plugin assembly, finds all plugin types within it, and initiates their processing.
        /// This method isolates assembly loading and error handling for each plugin file.
        /// </summary>
        /// <param name="dll">The plugin DLL file to process.</param>
        /// <param name="builder">The WebApplicationBuilder instance for service injection.</param>
        private static void LoadAndProcessPlugin(SystemFile dll, WebApplicationBuilder builder)
        {
            Loggers.PluginLogger.Debug("Attempting to load plugin assembly: {@DllName}", dll.Name);
            try
            {
                var loadContext = new PluginLoadContext(dll.FullPath);
                Assembly assembly = loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(dll.FullPath)));
                Loggers.PluginLogger.Debug("Successfully loaded assembly: {@AssemblyName}", assembly.FullName);

                IEnumerable<Type> pluginTypes = assembly.GetPluginTypes(typeof(IPluginBase));
                foreach (Type pluginType in pluginTypes)
                {
                    InstantiateAndConfigurePlugin(pluginType, assembly, dll.Name, builder);
                }
            }
            catch (ReflectionTypeLoadException ex) { HandleReflectionTypeLoadException(ex, dll.Name); }
            catch (FileLoadException ex) { Loggers.PluginLogger.Error(ex, "Error loading assembly file {@DllName}", dll.Name); }
            catch (BadImageFormatException ex) { Loggers.PluginLogger.Error(ex, "Error loading assembly {@DllName}: Invalid assembly format", dll.Name); }
            catch (Exception ex) { Loggers.PluginLogger.Error(ex, "An unexpected error occurred while processing plugin assembly {@DllName}", dll.Name); }
        }

        /// <summary>
        /// Provides detailed logging for ReflectionTypeLoadException, which occurs when some types in an assembly cannot be loaded.
        /// </summary>
        /// <param name="ex">The exception instance.</param>
        /// <param name="dllName">The name of the DLL that failed to load.</param>
        private static void HandleReflectionTypeLoadException(ReflectionTypeLoadException ex, string dllName)
        {
            Loggers.PluginLogger.Error(ex, "Error loading types from assembly {@DllName}", dllName);
            if (ex.LoaderExceptions != null)
            {
                foreach (Exception? loaderEx in ex.LoaderExceptions)
                {
                    Loggers.PluginLogger.Error(loaderEx, "LoaderException loading {@DLL}", dllName);
                }
            }
        }

        /// <summary>
        /// Creates an instance of a discovered plugin type, injects services if required, and registers the plugin.
        /// </summary>
        /// <param name="pluginType">The plugin type to instantiate.</param>
        /// <param name="assembly">The assembly the plugin belongs to.</param>
        /// <param name="dllName">The name of the DLL file for logging context.</param>
        /// <param name="builder">The WebApplicationBuilder for service injection.</param>
        private static void InstantiateAndConfigurePlugin(Type pluginType, Assembly assembly, string dllName, WebApplicationBuilder builder)
        {
            Loggers.PluginLogger.Information("Found plugin type: {@PluginType} in {@DllName}", pluginType.FullName, dllName);
            try
            {
                if (Activator.CreateInstance(pluginType) is not IPluginBase pluginInstance)
                {
                    Loggers.PluginLogger.Warning("Could not create an instance of plugin type: {@PluginType} in {@DllName}.", pluginType.FullName, dllName);
                    return;
                }

                // Why: Inject services if the plugin supports it.
                if (pluginInstance is IPluginServiceProvider pluginServices)
                {
                    Loggers.PluginLogger.Information("Injecting services for plugin: {@PluginType}", pluginType.FullName);
                    pluginServices.InjectServices(builder);
                }

                // Why: Add the initialized plugin to the application's list of loaded plugins for runtime access.
                ApplicationInfo.loadedPlugins.Add(new(assembly, pluginInstance));
                Loggers.PluginLogger.Information("Successfully loaded and injected services for plugin: {@PluginType}", pluginType.FullName);
            }
            catch (Exception ex)
            {
                Loggers.PluginLogger.Error(ex, "Error creating instance or injecting services for plugin {@PluginType} in {@DllName}", pluginType.FullName, dllName);
            }
        }



        // Lock object for thread safety when modifying shared service collection in parallel
        private static readonly object _lock = new();

        /// <summary>
        /// Discovers and registers background services marked with the <see cref="AutoStartBackgroundService"/> attribute
        /// from all loaded BLAZAM assemblies.
        /// </summary>
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
                        Loggers.SendToSeqServer = appSettings.SendLogsToDeveloper;
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
                if (ApplicationInfo.installationCompleted)
                {
                    Loggers.SystemLogger.Information("Installation complete. Starting background services...");
                    StartBackgroundServices(application);
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

            PreloadNotificationGenerationService(application);
            InitializeUpdateService(application);
            PreloadWebHookPublisher(application);
            StartApplicationStatisticsPolling();

            Loggers.SystemLogger.Information("Finished preloading/starting singleton services.");
        }

        private static void StartBackgroundServices(WebApplication application)
        {
            foreach (var assembly in blazamAssemblies)
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => t.IsClass && !t.IsAbstract
                        && t.GetCustomAttribute<AutoStartBackgroundService>() != null);

                    foreach (var type in types)
                    {
                        TryStartBackgroundService(application, type, assembly);
                    }
                }
                catch (Exception ex)
                {
                    Loggers.SystemLogger.Error(ex, "Error finding or processing background service types in assembly {AssemblyName}", assembly.FullName);
                }
            }
        }

        private static void TryStartBackgroundService(WebApplication application, Type type, Assembly assembly)
        {
            try
            {
                var interfaceType = type.GetInterfaces()
                    .FirstOrDefault(i => i.GetCustomAttribute<AutoStartBackgroundService>() == null && i.Name != "IDisposable");

                object? resolvedService = interfaceType != null
                    ? application.Services.GetService(interfaceType)
                    : application.Services.GetService(type);

                var service = resolvedService as BackgroundServiceBase;
                if (service != null)
                {
                    var metadata = type.GetCustomAttribute<AutoStartBackgroundService>();
                    Loggers.SystemLogger.Information("Starting background service: {ServiceType} (Immediate: {ImmediateStart})", type.FullName, metadata?.Immediate == true);
                    if (metadata?.RunOnLinux == true || !OperatingSystem.IsLinux())
                    {
                        service.Start(metadata?.Immediate == true);
                    }
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

        private static void PreloadNotificationGenerationService(WebApplication application)
        {
            try
            {
                Loggers.SystemLogger.Debug("Preloading NotificationGenerationService...");
                _ = application.Services.GetRequiredService<NotificationGenerationService>();
                Loggers.SystemLogger.Debug("NotificationGenerationService preloaded.");
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex, "Error preloading NotificationGenerationService.");
            }
        }

        private static void InitializeUpdateService(WebApplication application)
        {
            try
            {
                if (ApplicationInfo.installationCompleted)
                {
                    Loggers.SystemLogger.Debug("Initializing UpdateService...");
                    var updateService = application.Services.GetRequiredService<UpdateService>();
                    updateService.Initialize();
                    Loggers.SystemLogger.Debug("UpdateService initialized.");
                }
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex, "Error initializing UpdateService.");
            }
        }

        private static void PreloadWebHookPublisher(WebApplication application)
        {
            try
            {
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
        }

        private static void StartApplicationStatisticsPolling()
        {
            try
            {
                if (ApplicationInfo.installationCompleted)
                {
                    Loggers.SystemLogger.Debug("Starting application statistics polling...");
                    ApplicationStatistics.Process = ApplicationInfo.runningProcess;
                    ApplicationStatistics.StartResourceUsagePolling();
                    Loggers.SystemLogger.Debug("Application statistics polling started.");
                }
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex, "Error starting application statistics polling.");
            }
        }
    }
}
