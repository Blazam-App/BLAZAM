
using BLAZAM.Common.Data.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using MudBlazor.Services;
using System.Globalization;
using MudBlazor;
using BLAZAM.Database.Context;
using BLAZAM.Session.Interfaces;
using BLAZAM.Notifications.Services;
using BLAZAM.Common.Data;
using BLAZAM.Services;
using BLAZAM.Services.Duo;
using System.Diagnostics;
using System.Reflection;
using BLAZAM.Services.Chat;
using BLAZAM.Services.Audit;
using BLAZAM.Nav;
using BLAZAM.Session;
using Microsoft.AspNetCore.Authentication;
using System.Management;

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
            //Set DebugMode flag from configuration
            ApplicationInfo ApplicationInfo = new(builder);
            ApplicationInfo.inDebugMode = builder.Configuration.GetValue<bool>("DebugMode");
            ApplicationInfo.inDemoMode = builder.Configuration.GetValue<bool>("DemoMode");

            //Set the installation ID
            try
            {
                //Attempts to get the windows installation GUID
                ApplicationInfo.installationId = GetInstallationId();


            }catch
            {
                //Default to a hash type method on the machine name
                ApplicationInfo.installationId = Environment.MachineName.ToGuid();

            }
            
            Program.AppDataDirectory = new SystemDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Blazam\\");


            //Store the configuration so other pages/objects can easily access it
            Program.Configuration = builder.Configuration;

            //Captures the running process
            ApplicationInfo.runningProcess = Process.GetCurrentProcess();

            //Gets the application version from he running assembly version
            ApplicationInfo.runningVersion = new ApplicationVersion(Assembly.GetExecutingAssembly()) ;




            return builder;
        }

        private static Guid GetInstallationId()
        {
            //Try and get os id
            try
            {
                string ComputerName = "localhost";
                ManagementScope Scope;
                Scope = new ManagementScope(String.Format("\\\\{0}\\root\\CIMV2", ComputerName), null);
                Scope.Connect();
                ObjectQuery Query = new ObjectQuery("SELECT UUID FROM Win32_ComputerSystemProduct");
                ManagementObjectSearcher Searcher = new ManagementObjectSearcher(Scope, Query);

                foreach (ManagementObject WmiObject in Searcher.Get())
                {
                    return Guid.Parse(WmiObject["UUID"].ToString());
                                     
                }
                throw new ApplicationException("Searched but could not find a CSProduct UUID");
            }
        
            catch (Exception ex)
                {
                    Console.WriteLine("Failed to get client ID (GUID). Error: " + ex.Message);
                    throw ex;
                }
           

        }

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
             
             
            CultureInfo culture = new CultureInfo("ru");
            //CultureInfo culture = new CultureInfo("zh-Hans");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
           */



            builder.Services.AddSingleton<ApplicationInfo>();



            //Set up authentication and api token authentication
            builder.Services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(AppAuthenticationStateProvider.ApplyAuthenticationCookieOptions());

            builder.Services.Configure<AuthenticationOptions>(options =>
            {
                options.RequireAuthenticatedSignIn = false;
            });
            /*
            Keeping  this here for a possible API in the future
            It's some original test code from before AppAuthenticatinProvider was
            completed so it may not be usable as is

            builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
            .AddNegotiate().AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateAudience = false,
                        ValidateIssuer = false,
                        ValidateActor = false,
                        ValidateLifetime = true,
                        IssuerSigningKey = TokenKey
                    };
            });
            
          
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
                .AddCircuitOptions(options => {
                    options.DetailedErrors = ApplicationInfo.inDebugMode;
                });

            //Inject the database as a service

            DatabaseContextBase.Configuration = builder.Configuration;

            builder.Services.AddSingleton<IAppDatabaseFactory,AppDatabaseFactory>();

            //Provide an Http client as a service with custom construction via api service class
            builder.Services.AddHttpClient();

            //Also keeping this here for a possible future API, though this would be for internal use
            //builder.Services.AddTransient<ApiService>();
            //builder.Services.AddTransient<IPrincipal>(provider => provider.GetService<IHttpContextAccessor>().HttpContext.User);

            //Provide a way to get the current HTTP userPrincipal as a service
            builder.Services.AddHttpContextAccessor();


            

            //Provide the email client as a service
            builder.Services.AddSingleton<EmailService>();


            //Provide chat as a service
            builder.Services.AddSingleton<IChatService,ChatService>();

            //Sets up Active Directory communications
            builder.Services.AddActiveDirectoryServices();


            //Provide an ApplicationManager as a service
            builder.Services.AddSingleton<ApplicationManager>();

            //Provide a PermissionHandler as a service
            builder.Services.AddSingleton<PermissionApplicator>();
            
            builder.Services.AddSingleton<UserSeederService>();

            builder.Services.AddSingleton<IApplicationNewsService, ApplicationNewsService>();

            //Provide a AuditLogger as a service
            builder.Services.AddScoped<AuditLogger>();




            //Add custom Auth
            builder.Services.AddScoped<AppAuthenticationStateProvider>();

            //Add web user application search as a service
            builder.Services.AddScoped<SearchService>();


            //A substitue Navigation Manager for the app to enable navigation warning on unsaved
            //changes
            builder.Services.AddScoped<AppNavigationManager>();






            //Provide DuoSecurity service
            builder.Services.AddScoped<IDuoClientProvider, DuoClientProvider>();

            //Provide encyption service
            //There's no benefit to filling memory with identical instances of this, so singleton
            builder.Services.AddSingleton<IEncryptionService, EncryptionService>();

            //Provide database and active directory monitoring service
            //This serivice runs a Timer, and so singleton
            builder.Services.AddSingleton<ConnMonitor>();


            //Provide notification publishing as a service
            builder.Services.AddSingleton<INotificationPublisher,NotificationPublisher>();




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

            builder.Services.AddSingleton<OUNotificationService>();


            builder.Host.UseWindowsService();

            return builder;
        }

        public static void PreRun(this WebApplication application)
        {
            //Setup Seq logging if allowed by admin
            try
            {
                var context = Program.AppInstance.Services.GetRequiredService<IAppDatabaseFactory>().CreateDbContext();
                if (context != null && context.AppSettings.FirstOrDefault()?.SendLogsToDeveloper != null)
                {
                    Loggers.SendToSeqServer = context.AppSettings.FirstOrDefault().SendLogsToDeveloper;

                }

            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex.Message + " {@Error}", ex);
            }
            PreloadServices();

        }

        private static void PreloadServices()
        {
            try
            {
                var context = Program.AppInstance.Services.GetRequiredService<OUNotificationService>();


            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex.Message + " {@Error}", ex);
            }
            try
            {
                if (ApplicationInfo.installationCompleted)
                {
                    var context = Program.AppInstance.Services.GetRequiredService<UserSeederService>();
                }

            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex.Message + " {@Error}", ex);
            }

        }
    }
}
