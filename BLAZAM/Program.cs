using BLAZAM.Server.Background;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Hosting.WindowsServices;
using System.Globalization;
using BLAZAM.Server.Middleware;
using BLAZAM.Server.Data.Services;
using BLAZAM.Server.Data.Services.Duo;
using BLAZAM.Common.Data.Services;
using BLAZAM.Common.Data.ActiveDirectory;
using BLAZAM.Common;
using BLAZAM.Server.Data;
using BLAZAM.Common.Data.Database;
using BLAZAM.Common.Data.ActiveDirectory.Interfaces;
using BLAZAM.Server.Data.Services.Email;
using BLAZAM.Server.Data.Services.Update;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Serilog;
using BLAZAM.Server.Pages.Error;
using System.Diagnostics;
using BLAZAM.Common.Data;
using MudBlazor.Services;
using BLAZAM.Server;

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
        internal static SystemDirectory TempDirectory { get; set; }

        public static SystemDirectory AppDataDirectory { get; set; }

        /// <summary>
        /// The process of the running application
        /// </summary>
        public static Process ApplicationProcess { get; set; }

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
        public static ConfigurationManager? Configuration { get; set; }
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

            builder.IntializeProperties();

            CheckWritablePathPermissions();


            //Setup host logging so it can catch the earliest logs possible

            Loggers.SetupLoggers(WritablePath + @"logs\");
            builder.Host.UseSerilog(Log.Logger);

            Log.Information("Application Starting");

            builder.InjectServices();


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
            }




            AppInstance.UseMiddleware<UserStateMiddleware>();
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

            StartDatabaseCache();

            AppInstance.Start();
            GetRunningWebServerConfiguration();
            ScheduleAutoLoad();

            AppInstance.WaitForShutdown();
            Log.Information("Application Shutting Down");
            //AppInstance.Run();

        }

        private static void ScheduleAutoLoad()
        {
            Task.Delay(5000).ContinueWith(t =>
            {
                new AutoLauncher(AppInstance.Services.GetService<IHttpClientFactory>());
            });
        }

        private static void GetRunningWebServerConfiguration()
        {
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
        }

        private static void StartDatabaseCache()
        {
            //Start the database cache
            using (var scope = AppInstance.Services.CreateScope())
            {
                _programDbFactory = scope.ServiceProvider.GetRequiredService<AppDatabaseFactory>();
                DatabaseCache.Start(_programDbFactory, Loggers.DatabaseLogger);
            }
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
            catch (DirectoryNotFoundException)
            {
                Writable = false;

                Oops.ErrorMessage = "Applicatin Directory Error";
                Oops.DetailsMessage = "The application's 'writable' directory is missing!";
            }

        }

        internal static async Task<bool> ApplyDatabaseMigrations(bool force = false)
        {

            return await Task.Run(() =>
            {
                try
                {
                    using (var scope = AppInstance.Services.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<AppDatabaseFactory>().CreateDbContext();
                        if (context != null && context.Status == ServiceConnectionState.Up)
                            if (context.IsSeeded() || force)
                                if (!context.SeedMismatch)
                                {
                                    if (context.Database.GetPendingMigrations().Count() > 0)
                                        context.Migrate();
                                }
                                else
                                {
                                    throw new DatabaseException("Database incompatible with current application version.");
                                }
                        //context.Database.Migrate();

                    }
                    return true;
                }
                catch(DatabaseException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    Loggers.DatabaseLogger.Error("Database Auto-Update Failed!!!!", ex);
                    throw ex;
                    return false;
                }
            });

        }

    }
}