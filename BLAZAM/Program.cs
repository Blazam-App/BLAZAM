using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Hosting.WindowsServices;
using System.Globalization;
using BLAZAM.Server.Middleware;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Serilog;
using System.Diagnostics;
using BLAZAM.Common.Data;
using MudBlazor.Services;
using BLAZAM.Server;
using BLAZAM.FileSystem;
using BLAZAM.Update;
using System.Reflection;
using BLAZAM.Database.Context;
using BLAZAM.Database.Exceptions;
using BLAZAM.Services.Background;

namespace BLAZAM
{


    public class Program
    {
      
        /// <summary>
        /// The writable directry
        /// </summary>
        /// <returns>
        /// eg: C:\inetpub\blazam\writable\
        /// </returns>
        internal static SystemDirectory WritablePath => new SystemDirectory(ApplicationInfo.tempDirectory + @"writable\");

    

        public static SystemDirectory AppDataDirectory { get; set; }



      
        private static IAppDatabaseFactory? _programDbFactory;







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


            //Build the application and allow it to run as a service
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions()
            {
                ContentRootPath = WindowsServiceHelpers.IsWindowsService() ? AppContext.BaseDirectory : default,
                Args = args
            });

            if (builder.Configuration is null)
                throw new ApplicationException("The appsettings.json configuration file was not loaded");

            builder.IntializeProperties();


            //Setup host logging so it can catch the earliest logs possible

            Loggers.SetupLoggers(WritablePath + @"logs\",ApplicationInfo.runningVersion.ToString());
            builder.Host.UseSerilog(Log.Logger);

            Log.Information("Application Starting");

            builder.InjectServices();


            //Done with service injection let's build the App
            AppInstance = builder.Build();

            ApplicationInfo.services = AppInstance.Services;






            AppInstance.UseSerilogRequestLogging(configureOptions => configureOptions.Logger = Loggers.RequestLogger);

            // Configure the HTTP request pipeline.
            if (!AppInstance.Environment.IsDevelopment() && !ApplicationInfo.inDebugMode)
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
                new AutoLauncher(AppInstance.Services.GetService<IHttpClientFactory>(), AppInstance.Services.GetService<ApplicationInfo>());
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
                        ApplicationInfo.listeningAddresses.Append(address);
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
                _programDbFactory = scope.ServiceProvider.GetRequiredService<IAppDatabaseFactory>();
                DatabaseCache.Start(_programDbFactory);
            }
        }


        public static bool IsDevelopment
        {
            get
            {
                return AppInstance.Environment.IsDevelopment();
            }
        }


        //public static void CheckWritablePathPermissions()
        //{

        //    try
        //    {
        //        //Check permissions
        //        File.WriteAllText(WritablePath + @"writetest.test", "writetest");
        //        Writable = true;
        //        File.Delete(WritablePath + @"writetest.test");

        //    }
        //    catch (UnauthorizedAccessException)
        //    {
        //        Writable = false;
        //        Oops.ErrorMessage = "Applicatin Directory Error";
        //        Oops.DetailsMessage = "The application does not have write permission to the 'writable' directory.";
        //    }
        //    catch (DirectoryNotFoundException)
        //    {
        //        Writable = false;

        //        Oops.ErrorMessage = "Applicatin Directory Error";
        //        Oops.DetailsMessage = "The application's 'writable' directory is missing!";
        //    }

        //}

      

    }
}