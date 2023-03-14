using BLAZAM.Server.Background;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Hosting.WindowsServices;
using Newtonsoft.Json;
using System.Globalization;
using Blazorise;
using Blazorise.Icons.FontAwesome;
using Blazorise.Bootstrap5;
using BLAZAM.Server.Middleware;
using BLAZAM.Server.Data.Services;
using BLAZAM.Server.Data.Services.Duo;
using BLAZAM.Common.Data.Services;
using BLAZAM.Common.Data.ActiveDirectory;
using BLAZAM.Common;
using BLAZAM.Server.Data;
using BLAZAM.Common.Data.Database;
using Microsoft.Extensions.Localization;
using BLAZAM.Server.Shared.ResourceFiles;
using BLAZAM.Common.Data.ActiveDirectory.Models;
using BLAZAM.Common.Data.ActiveDirectory.Interfaces;
using Microsoft.AspNetCore.Components.Server.Circuits;
using BLAZAM.Server.Data.Services.Email;
using BLAZAM.Server.Data.Services.Update;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using BLAZAM.Server;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Serilog;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Serilog.Events;
using Blazorise.RichTextEdit;
using BLAZAM.Server.Pages.Error;
using System.Diagnostics;
using System.Reflection;
using BLAZAM.Common.Models.Database;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding;

namespace BLAZAM
{


    public class Program
    {
        /// <summary>
        /// The root directory of the running web application
        /// </summary>
        /// <returns>
        /// eg: C:\inetpub\blazam\
        /// </returns>
        /// 
        internal static SystemDirectory RootDirectory { get; set; }
        /// <summary>
        /// The writable directry
        /// </summary>
        /// <returns>
        /// eg: C:\inetpub\blazam\writable\
        /// </returns>
        internal static SystemDirectory WritablePath => new SystemDirectory(Program.TempDirectory + @"writable\");
        /// <summary>
        /// The temporary file directry
        /// </summary>
        /// <returns>
        /// eg: C:\Users\user\appdata\temp\
        /// </returns>
        internal static SystemDirectory TempDirectory { get; private set; }
        public static SystemDirectory AppDataDirectory { get; private set; }

        /// <summary>
        /// The process of the running application
        /// </summary>
        public static Process ApplicationProcess { get; private set; }
        /// <summary>
        /// The running Blazam version
        /// </summary>
        internal static ApplicationVersion Version { get; } = new ApplicationVersion();
        /// <summary>
        /// A collection of active listening address's with port
        /// </summary>
        /// <returns>
        /// A list of address strings eg: {"https://localhost:7900/","http://localhost:5900/"}
        /// </returns>
        internal static List<string> ListeningAddresses { get; private set; } = new List<string>();

        private static AppDatabaseFactory? _programDbFactory;



        static bool? installationCompleted = null;
        /// <summary>
        /// Indicates the Installation status
        /// </summary>
        internal static bool InstallationCompleted
        {
            get
            {
                using (var context = _programDbFactory?.CreateDbContext())
                {
                    if (installationCompleted != true && context != null)
                    {
                        if (context.IsSeeded())
                        {
                            try
                            {
                                var appSettings = context.AppSettings.FirstOrDefault();
                                if (appSettings != null)
                                    installationCompleted = appSettings.InstallationCompleted;
                                else
                                    installationCompleted = false;
                            }
                            catch (Exception ex)
                            {
                                Loggers.DatabaseLogger.Error("There was an error checking the installation flag in the database.", ex);
                            }
                            //if (!context.Seeded()) installationCompleted = false;
                            //else installationCompleted = (DatabaseCache.ApplicationSettings?.InstallationCompleted == true);
                        }
                        else
                            installationCompleted = false;


                    }
                    return installationCompleted != false;
                }
            }
            set
            {
                installationCompleted = value;
            }
        }

        /// <summary>
        /// Flag for checking if the application is running in Debug Mode.
        /// </summary>
        internal static bool InDebugMode;

        /// <summary>
        /// Flag for checking if the application is running in Demo Mode.
        /// </summary>
        internal static bool InDemoMode;

        /// <summary>
        /// Can be used for JWT Token signing
        /// </summary>
        internal static SymmetricSecurityKey? TokenKey;

        /// <summary>
        /// A static reference for the current asp
        /// net core application instance
        /// </summary>
        public static WebApplication AppInstance { get; private set; }
        /// <summary>
        /// A static reference to the asp net 
        /// core application configuration
        /// </summary>
        public static ConfigurationManager? Configuration { get; private set; }
        /// <summary>
        /// Indicates whether the Account running the website can wrrite to the writable path
        /// </summary>
        public static bool Writable { get; private set; }



        /// <summary>
        /// The main injection point for the web application.
        /// It all starts here.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            Console.WriteLine("Working Directory: " + Environment.CurrentDirectory);

            ApplicationProcess = Process.GetCurrentProcess();

            //Build the application and allow it to run as a service
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions()
            {
                ContentRootPath = WindowsServiceHelpers.IsWindowsService() ? AppContext.BaseDirectory : default,
                Args = args
            });


            //Set DebugMode flag from configuration
            InDebugMode = builder.Configuration.GetValue<bool>("DebugMode");
            InDemoMode = builder.Configuration.GetValue<bool>("DemoMode");


            //Set application directories
            RootDirectory = new SystemDirectory(builder.Environment.ContentRootPath);
            TempDirectory = new SystemDirectory(Path.GetTempPath() + "Blazam\\");
            AppDataDirectory = new SystemDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Blazam\\");


            //Store the configuration so other pages/objects can easily access it
            Configuration = builder.Configuration;


            CheckWritablePathPermissions();


            //Setup host logging so it can catch the earliest logs possible

            Loggers.SetupLoggers(WritablePath + @"logs\");
            builder.Host.UseSerilog(Log.Logger);

            Log.Information("Application Starting");



            //Set up string localization
            builder.Services.AddLocalization();
            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("en-US"),
                    new CultureInfo("fr-FR")
                 };

                options.DefaultRequestCulture = new RequestCulture("fr-FR");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });
            /*
             * Uncomment this to force a language
             * 
            CultureInfo culture = new CultureInfo("fr-FR");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            */

            //Grab the connection string and store it in the context statically
            //This can obviously only be changed on app restart





            //Add the httpcontext to services so we can detect the users login status
            builder.Services.AddHttpContextAccessor();

            //Set up authentication and api token authentication
            builder.Services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            var test = builder.Services.AddAuthentication(
                CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {

                    options.Events.OnCheckSlidingExpiration = async (context) =>
                    {
                        if (DatabaseCache.AuthenticationSettings?.SessionTimeout != null)
                            context.Options.ExpireTimeSpan = TimeSpan.FromMinutes((double)DatabaseCache.AuthenticationSettings.SessionTimeout);
                        var shouldRenew = context.ShouldRenew;
                    };
                    options.Events.OnSigningIn = async (context) =>
                    {
                        //Use session timeout from settings in database
                        if (DatabaseCache.AuthenticationSettings.SessionTimeout != null)
                            context.Options.ExpireTimeSpan = TimeSpan.FromMinutes((double)DatabaseCache.AuthenticationSettings.SessionTimeout);

                    };
                    options.LoginPath = new PathString("/login");
                    options.LogoutPath = new PathString("/logout");
                    options.ExpireTimeSpan = TimeSpan.FromSeconds(10);
                    options.SlidingExpiration = true;
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


            //Enable razor pages
            builder.Services.AddRazorPages();

            //Run as server side blazor with detailed errors controlled by DebugMode configuration
            builder.Services.AddServerSideBlazor().AddCircuitOptions(options => { options.DetailedErrors = InDebugMode; });

            //Inject the database as a service

            DatabaseContext.Configuration = builder.Configuration;
            
            builder.Services.AddSingleton<AppDatabaseFactory>();

            //builder.Services.AddDbContextFactory<DatabaseContext>();




            //Provide an Http client as a service with custom construction via api service class
            builder.Services.AddHttpClient();
            //Also keeping this here for a possible future API, though this would be for internal use
            //builder.Services.AddTransient<ApiService>();
            //builder.Services.AddTransient<IPrincipal>(provider => provider.GetService<IHttpContextAccessor>().HttpContext.User);

            //Provide a way to get the current HTTP userPrincipal as a service
            builder.Services.AddHttpContextAccessor();

            //This probably don't need to be here
            builder.Services.AddSingleton<WmiFactoryService>();

            //Provide updating as a service, may be a little much for one page using it
            builder.Services.AddSingleton<UpdateService>();

            //Provide the email client as a service
            builder.Services.AddSingleton<EmailService>();


            //Provide a primary Active Directory connection as a service
            //We run this as a singleton so each user connection doesn't have to wait for connection verification to happen
            builder.Services.AddSingleton<IActiveDirectory, ActiveDirectoryContext>();



            //Provide an ApplicationManager as a service
            builder.Services.AddScoped<ApplicationManager>();

            //Provide a PermissionHandler as a service
            builder.Services.AddScoped<LoginPermissionApplicator>();

            //Provide a AuditLogger as a service
            builder.Services.AddScoped<AuditLogger>();


            //Add custom Auth
            builder.Services.AddScoped<AppAuthenticationStateProvider, AppAuthenticationStateProvider>();

            //Add web user application search as a service
            builder.Services.AddScoped<SearchService>();


            //Provide DuoSecurity service
            builder.Services.AddSingleton<IDuoClientProvider, DuoClientProvider>();

            //Provide encyption service
            //There's no benefit to filling memory with identical instances of this, so singleton
            builder.Services.AddSingleton<IEncryptionService, EncryptionService>();

            //Provide database and active directory monitoring service
            //This serivice runs a Timer, and so singleton
            builder.Services.AddSingleton<ConnMonitor>();

            //Provide UserStates as a service
            //This service is a "hack" for Blazor Server not having, in a real sense, sessions
            //It allows data to persist between refreshes/reloading page navigations per logged
            //in user principal
            builder.Services.AddSingleton<IApplicationUserStateService, ApplicationUserStateService>();

            //Provide Automatic Updates as a service
            //This service runs checks every 4 hours for an update and if found, schedules an
            //update at a time of day specified in the database
            builder.Services.AddSingleton<AutoUpdateService>();




            //builder.Services.AddBlazoredSessionStorage(); 
            //builder.Services.AddScoped<LoginRedirector>();

            //Add Blazorize UI framework

            builder.Services.AddBlazorise(options =>
                {
                    options.Immediate = true;

                })
                .AddBootstrap5Providers()
                .AddFontAwesomeIcons()
                .AddLogging();


            builder.Services.AddBlazoriseRichTextEdit();


            builder.Host.UseWindowsService();


            //Done with service injection let's build the App
            AppInstance = builder.Build();


            //Perform database auto update
            ApplyDatabaseMigrations();




            AppInstance.UseSerilogRequestLogging(configureOptions => configureOptions.Logger = Loggers.RequestLogger);

            // Configure the HTTP request pipeline.
            if (!AppInstance.Environment.IsDevelopment() && !InDebugMode)
            {

                AppInstance.UseExceptionHandler("/Error");

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                AppInstance.UseHsts();
            }
            else
            {
                AppInstance.Environment.EnvironmentName = "Development";
                AppInstance.UseDeveloperExceptionPage();
                AppInstance.UseDeveloperExceptionPage();
            }





            AppInstance.UseMiddleware<HttpsRedirectionMiddleware>();
            AppInstance.UseMiddleware<ApplicationStatusRedirectMiddleware>();
            AppInstance.UseStaticFiles();
            AppInstance.UseRouting();
            AppInstance.UseCookiePolicy();
            AppInstance.UseAuthentication();
            AppInstance.UseAuthorization();

            //AppInstance.MapControllers();
            AppInstance.MapBlazorHub();
            AppInstance.MapFallbackToPage("/_Host");


            //Start the database cache
            using (var scope = AppInstance.Services.CreateScope())
            {
                _programDbFactory = scope.ServiceProvider.GetRequiredService<AppDatabaseFactory>();
                DatabaseCache.Start(_programDbFactory, Loggers.DatabaseLogger);
            }


            AppInstance.Start();
            var server = AppInstance.Services.GetService<IServer>();
            if (server != null)
            {
                var addressFeature = server.Features.Get<IServerAddressesFeature>();
                if (addressFeature != null)
                {

                    foreach (var address in addressFeature.Addresses)
                    {
                        ListeningAddresses.Add(address);
                        Loggers.SystemLogger.Debug("Listening on: " + address);
                    }
                }
            }
            Task.Delay(5000).ContinueWith(t =>
            {
                new AutoLauncher(AppInstance.Services.GetService<IHttpClientFactory>());
            });

            AppInstance.WaitForShutdown();
            Log.Information("Application Shutting Down");
            //AppInstance.Run();

        }

        public static bool IsDevelopment
        {
            get
            {
                return AppInstance.Environment.IsDevelopment();
            }
        }


        public static void CheckWritablePathPermissions()
        {

            try
            {
                //Check permissions
                File.WriteAllText(WritablePath + @"writetest.test", "writetest");
                Writable = true;
                File.Delete(WritablePath + @"writetest.test");

            }
            catch (UnauthorizedAccessException)
            {
                Writable = false;
                Oops.ErrorMessage = "Applicatin Directory Error";
                Oops.DetailsMessage = "The application does not have write permission to the 'writable' directory.";
            }
            catch (DirectoryNotFoundException )
            {
                Writable = false;

                Oops.ErrorMessage = "Applicatin Directory Error";
                Oops.DetailsMessage = "The application's 'writable' directory is missing!";
            }

        }

        internal static async Task<bool> ApplyDatabaseMigrations(bool force = false)
        {

            return await Task.Run(() => {
                try
                {
                    using (var scope = AppInstance.Services.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<AppDatabaseFactory>().CreateDbContext();
                        if (context != null)
                            if (context.IsSeeded() || force)
                                if (context.Database.GetPendingMigrations().Count() > 0)
                                    context.Database.Migrate();

                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Loggers.DatabaseLogger.Error("Database Auto-Update Failed!!!!", ex);
                    return false;
                }
            });
           
        }

    }
}