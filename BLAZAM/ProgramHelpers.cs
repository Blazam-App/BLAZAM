using BLAZAM.ActiveDirectory.Services;
using BLAZAM.Common.Attributes;
using BLAZAM.Common.Conventions;
using BLAZAM.Common.Data;
using BLAZAM.Common.Data.Services;
using BLAZAM.Common.Exceptions;
using BLAZAM.Database.Context;
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
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MudBlazor;
using MudBlazor.Services;
using NuGet.Protocol.Plugins;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using System.Diagnostics;
using System.Globalization;
using System.Management;
using System.Reflection;

namespace BLAZAM.Server
{
    public static class ProgramHelpers
    {

        /// <summary>
        /// Sets up the core configuration like debug, installation id, and running process and version
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static WebApplicationBuilder IntializeProperties(this WebApplicationBuilder builder)
        {

            AppDomain.CurrentDomain.SetData("REGEX_DEFAULT_MATCH_TIMEOUT", TimeSpan.FromMilliseconds(100)); // process-wide setting

            //Set DebugMode flag from configuration
            ApplicationInfo ApplicationInfo = new(builder);
            ApplicationInfo.inDebugMode = builder.Configuration.GetValue<bool>("DebugMode");
            ApplicationInfo.inDemoMode = builder.Configuration.GetValue<bool>("DemoMode");

            //Set the installation ID
            try
            {
                //Attempts to get the windows installation GUID
                ApplicationInfo.installationId = GetInstallationId();


            }
            catch
            {
                //Default to a hash type method on the machine name
                ApplicationInfo.installationId = Environment.MachineName.ToGuid();

            }

            Program.AppDataDirectory = new SystemDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Blazam\\");
            Program.PluginDirectory = new SystemDirectory(Program.WritablePath + @"plugins\");


            //Store the configuration so other pages/objects can easily access it
            Program.Configuration = builder.Configuration;

            //Captures the running process
            ApplicationInfo.runningProcess = Process.GetCurrentProcess();

            //Gets the application version from he running assembly version
            ApplicationInfo.runningVersion = new ApplicationVersion(Assembly.GetExecutingAssembly());




            return builder;
        }
        /// <summary>
        /// Attempts to get the windows installation id 
        /// </summary>
        /// <returns>A unique GUID for this machine</returns>
        /// <exception cref="AppException">Thrown when the running context does not have read permission for the Windows UUID</exception>
        private static Guid GetInstallationId()
        {
            //Try and get os id
            try
            {
                string ComputerName = "localhost";
                ManagementScope Scope;
                Scope = new ManagementScope(String.Format("\\\\{0}\\root\\CIMV2", ComputerName), null);
                Scope.Connect();
                ObjectQuery Query = new("SELECT UUID FROM Win32_ComputerSystemProduct");
                ManagementObjectSearcher Searcher = new(Scope, Query);

                foreach (ManagementObject WmiObject in Searcher.Get())
                {
                    try
                    {
                        return Guid.Parse(WmiObject["UUID"].ToString());
                    }
                    catch
                    {
                        continue;
                    }

                }
                throw new AppException("Searched but could not find a CSProduct UUID");
            }

            catch (Exception ex)
            {
                Console.WriteLine("Failed to get client ID (GUID). Error: " + ex.Message);
                throw ex;
            }


        }
        /// <summary>
        /// Injects neccessary services for Blazam
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static WebApplicationBuilder InjectServices(this WebApplicationBuilder builder)
        {

            //Set up string localization
            builder.Services.AddLocalization();
            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("en-US"),
                    new CultureInfo("fr-FR"),
                    new CultureInfo("de"),
                    new CultureInfo("es"),
                    new CultureInfo("hi"),
                    new CultureInfo("it"),
                    new CultureInfo("ja"),
                    new CultureInfo("ko"),
                    new CultureInfo("pl"),
                    new CultureInfo("ru"),
                    new CultureInfo("zh-Hans")

                 };

                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });
            /*
             * Uncomment this to force a language
           
            
            CultureInfo culture = new CultureInfo("zh-Hans");
            //CultureInfo culture = new CultureInfo("zh-Hans");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            
           */



            builder.Services.AddSingleton<ApplicationInfo>();


            // Set up authentication and API token authentication
            builder.Services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;

            }).AddCookie(AppAuthenticationStateProvider.ApplyAuthenticationCookieOptions())
            .AddJwtBearer(options =>  // Configure JWT Bearer here
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true, // Important: Validate the signing key
                    IssuerSigningKey = new SymmetricSecurityKey(Encryption.Instance.APITokenKey),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateActor = false,
                    ValidateLifetime = true,

                };
                options.Events = new JwtAuthenticationEventsHandler(
                    builder.Services.BuildServiceProvider().GetRequiredService<IApplicationUserStateService>(),
                    builder.Services.BuildServiceProvider().GetRequiredService<IAppDatabaseFactory>()
                );
            });
            builder.Services.Configure<AuthenticationOptions>(options =>
            {
                options.RequireAuthenticatedSignIn = false;
            });
            /*
              builder.Services.AddAuthorization(options =>
              {
                  // By default, all incoming requests will be authorized according to the default policy.
                  options.FallbackPolicy = options.DefaultPolicy;
              });
              */

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(10);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            //Enable razor pages
            builder.Services.AddRazorPages();

            //Run as server side blazor with detailed errors controlled by DebugMode configuration
            builder.Services.AddServerSideBlazor()
                .AddCircuitOptions(options =>
                {
                    options.DetailedErrors = ApplicationInfo.inDebugMode;
                });

            //Inject the database as a service

            DatabaseContextBase.Configuration = builder.Configuration;



            builder.Services.AddSingleton<IAppDatabaseFactory, AppDatabaseFactory>();
            builder.Services.AddScoped<IUserDatabaseFactory, UserDatabaseFactory>();

            //Provide an Http client as a service with custom construction via api service class
            builder.Services.AddHttpClient();

            builder.Services.AddHttpClient(HttpClientNames.WebHookHttpClientName)
                    .SetHandlerLifetime(TimeSpan.FromMinutes(5))  //Set lifetime to five minutes
                    .AddPolicyHandler(GetWebhookRetryPolicy());

            builder.Services.AddHttpClient(HttpClientNames.WebHookHttpClientNoSSLCheckName)
                  .SetHandlerLifetime(TimeSpan.FromMinutes(5))  //Set lifetime to five minutes
                  .AddPolicyHandler(GetWebhookRetryPolicy())
                  .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                  {
                      ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
                  });


            //Provide a way to get the current HTTP userPrincipal as a service
            builder.Services.AddHttpContextAccessor();




            //Provide the email client as a service
            builder.Services.AddSingleton<EmailService>();


            //Provide chat as a service
            builder.Services.AddSingleton<IChatService, ChatService>();

            //Sets up Active Directory communications
            builder.Services.AddActiveDirectoryServices();


            //Provide an ApplicationManager as a service
            builder.Services.AddSingleton<ApplicationManager>();

            //Provide a PermissionHandler as a service
            builder.Services.AddScoped<PermissionApplicator>();


            //Provide a AuditLogger as a service
            builder.Services.AddScoped<WebUserAuditLogger>();
            builder.Services.AddSingleton<ServerAuditLogger>();

            builder.Services.AddScoped<GoogleAuthenticatorService>();

            //Provide a JwtTokens as a service
            builder.Services.AddScoped<JwtTokenService>();

            //Provide a JwtTokens as a service
            builder.Services.AddSingleton<WebHookPublisher>();




            //Add custom Auth
            builder.Services.AddScoped<AppAuthenticationStateProvider>();

            //Add web user application search as a service
            builder.Services.AddScoped<SearchService>();



            //A service to provide the appropriate widgets to users, based on permissions
            builder.Services.AddScoped<WidgetService>();






            //Provide DuoSecurity service
            builder.Services.AddSingleton<IDuoClientProvider, DuoClientProvider>();

            //Provide encryption service
            //There's no benefit to filling memory with identical instances of this, so singleton
            builder.Services.AddSingleton<IEncryptionService, EncryptionService>();

            //Provide database and active directory monitoring service
            //This service runs a Timer, and so singleton
            builder.Services.AddSingleton<ConnMonitor>();


            //Provide notification publishing as a service
            builder.Services.AddSingleton<INotificationPublisher, NotificationPublisher>();

            builder.InjectBackgroundServices();

            builder.Services.AddSessionServices();


            builder.Services.AddUpdateServices();



            builder.Services.AddMudServices(configuration =>
            {
                configuration.SnackbarConfiguration.HideTransitionDuration = 250;
                configuration.SnackbarConfiguration.ShowTransitionDuration = 250;
            });

            builder.Services.AddMudMarkdownServices();


            builder.Services.AddScoped<AppSnackBarService>();

            builder.Services.AddScoped<AppDialogService>();

            builder.Services.AddSingleton<NotificationGenerationService>();

            builder.Services.AddControllers(options =>
            {

                options.Conventions.Add(new LowercaseControllerRouteConvention());
            });

            builder.Services.AddMvc();



            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
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

                // Add descriptions using XML comments
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

                // Configure Swagger to use JWT Bearer authorization
                var jwtSecurityScheme = new OpenApiSecurityScheme
                {
                    Description = "Enter only the token supplied by Blazam",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };

                c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement() {
                    { jwtSecurityScheme,Array.Empty<string>() }
                });

            });

            builder.Host.UseWindowsService();

            InjectPluginServices(builder);

            return builder;
        }

        private static void InjectPluginServices(WebApplicationBuilder builder)
        {
            var pluginDir = Program.PluginDirectory;
            
            Parallel.ForEach(pluginDir.Files.Where(f=>f.Extension.Equals(".dll")), dll=>
            {
                try
                {
                    Assembly assembly = Assembly.LoadFrom(dll.FullPath);

                    // Get all types in the assembly that implement IPluginBase
                    var pluginTypes = assembly.GetTypes()
                        .Where(type => typeof(IPluginBase).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);

                    foreach (Type pluginType in pluginTypes)
                    {
                        try
                        {
                            // Create an instance of the plugin
                            if (Activator.CreateInstance(pluginType) is IPluginBase pluginInstance)
                            {
                                // Invoke the InjectServices method
                                pluginInstance.InjectServices(builder);
                            }
                            else
                            {
                                Loggers.SystemLogger.Warning($"Warning: Could not create an instance of plugin type: {pluginType.FullName} in {dll.Name}.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Loggers.SystemLogger.Error($"Error creating or injecting services for plugin {pluginType.FullName} in {dll.Name}: {ex.Message}");
                            // Optionally log the full exception: Console.WriteLine(ex);
                        }
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Loggers.SystemLogger.Error($"Error loading assembly {dll.Name}: {ex.Message}");
                    if (ex.LoaderExceptions != null)
                    {
                        foreach (Exception loaderEx in ex.LoaderExceptions)
                        {
                            Loggers.SystemLogger.Error($"- {loaderEx.Message}");
                        }
                    }
                }
                catch (FileLoadException ex)
                {
                    Loggers.SystemLogger.Error($"Error loading assembly {dll.Name}: {ex.Message}");
                }
                catch (BadImageFormatException ex)
                {
                    Loggers.SystemLogger.Error($"Error loading assembly {dll.Name}: Invalid assembly format - {ex.Message}");
                }
                catch (Exception ex)
                {
                    Loggers.SystemLogger.Error($"An unexpected error occurred while processing assembly {dll.Name}: {ex.Message}");
                }
            });
        }

        private static readonly object _lock = new();
        /// <summary>
        /// Injects all services in all BLAZAM assemblies that have the <see cref="AutoStartBackgroundService"/> attribute
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static WebApplicationBuilder InjectBackgroundServices(this WebApplicationBuilder builder)
        {

            Parallel.ForEach(blazamAssemblies, assembly =>
             {
                 var types = assembly.GetTypes()
                     .Where(t => t.IsClass && !t.IsAbstract
                     && t.GetCustomAttribute<AutoStartBackgroundService>() != null);

                 foreach (var type in types)
                 {
                     var interfaceType = type.GetInterfaces()
                                                 .FirstOrDefault(i => i.GetCustomAttribute<AutoStartBackgroundService>() == null
                                                 && i.Name != "IDisposable");

                     if (interfaceType != null)
                     {
                         lock (_lock)
                         {
                             builder.Services.AddSingleton(interfaceType, type);
                         }
                     }
                     else
                     {
                         lock (_lock)
                         {
                             builder.Services.AddSingleton(type);
                         }
                     }
                 }
             });

            return builder;
        }

        /// <summary>
        /// Auto-starts <see cref="AutoStartBackgroundService"/> adorned classes and
        /// sets the Seq logging  opt-out, and other singleton services
        /// </summary>
        /// <param name="application"></param>
        public static void PreRun(this WebApplication application)
        {

            //Setup Seq logging if allowed by admin
            try
            {
                using var context = Program.AppInstance.Services.GetRequiredService<IAppDatabaseFactory>().CreateDbContext();
                if (context != null && context.AppSettings.FirstOrDefault()?.SendLogsToDeveloper != null)
                {
                    Loggers.SendToSeqServer = context.AppSettings.FirstOrDefault()?.SendLogsToDeveloper != false;

                }

            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex.Message + " {@Error}", ex);
            }
            PreloadServices(application);

        }

        private static IAsyncPolicy<HttpResponseMessage> GetWebhookRetryPolicy()
        {
            var delay = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(1), retryCount: 5);

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(delay);
        }
        private static IEnumerable<Assembly> blazamAssemblies => AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName?.Contains("BLAZAM") == true);
        private static void PreloadServices(WebApplication application)
        {

            try
            {
                if (ApplicationInfo.installationCompleted)
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
                                try
                                {
                                    var interfaceType = type.GetInterfaces()
                                        .FirstOrDefault(i => i.GetCustomAttribute<AutoStartBackgroundService>() == null
                                        && i.Name != "IDisposable");
                                    BackgroundServiceBase? service;

                                    if (interfaceType != null)
                                    {
                                        service = application.Services.GetRequiredService(interfaceType) as BackgroundServiceBase;

                                    }
                                    else
                                    {
                                        service = application.Services.GetRequiredService(type) as BackgroundServiceBase;

                                    }


                                    var metadata = type.GetCustomAttribute<AutoStartBackgroundService>();

                                    service?.Start(metadata?.Immediate == true);
                                }
                                catch (Exception ex)
                                {
                                    Loggers.SystemLogger.Error("Critical error starting up background service! {Error}", ex);

                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Loggers.SystemLogger.Warning("Unexpected error starting up background service! {Error}", ex);

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error("Critical error getting loaded assemblies! {Error}", ex);

            }
            try
            {
                var context = Program.AppInstance.Services.GetRequiredService<NotificationGenerationService>();
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex.Message + " {@Error}", ex);
            }
            try
            {
                if (ApplicationInfo.installationCompleted)
                {
                    var context = Program.AppInstance.Services.GetRequiredService<UpdateService>();
                    context.Initialize();
                }

            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex.Message + " {@Error}", ex);
            }
            try
            {
                if (ApplicationInfo.installationCompleted)
                {
                    var context = Program.AppInstance.Services.GetRequiredService<WebHookPublisher>();

                }

            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex.Message + " {@Error}", ex);
            }
            try
            {
                if (ApplicationInfo.installationCompleted)
                {
                    ApplicationStatistics.Process = ApplicationInfo.runningProcess;
                    ApplicationStatistics.StartResourceUsagePolling();
                }

            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex.Message + " {@Error}", ex);
            }
        }
    }
}
