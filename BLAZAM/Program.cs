
using Microsoft.Extensions.Hosting.WindowsServices;
using BLAZAM.Server.Middleware;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Serilog;
using BLAZAM.Common.Data;
using BLAZAM.Server;
using BLAZAM.Services.Background;
using System.Net;
using BLAZAM.Database.Context;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Net.WebSockets;
using BLAZAM.Database.Models;

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











        /// <summary>
        /// A static reference for the current ASP
        /// Net Core application instance
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

            _ = new Encryption(Configuration.GetValue<string>("EncryptionKey"));

            //Setup host logging so it can catch the earliest logs possible

            Loggers.SetupLoggers(WritablePath + @"logs\", ApplicationInfo.runningVersion.ToString());
            builder.Host.UseSerilog(Log.Logger);

            Log.Warning("Application Starting {@ProcessName}", ApplicationInfo.runningProcess.ProcessName);

            builder.InjectServices();



            builder.Services.AddCors();
            
            
            SetupKestrel(builder);


            //Done with service injection let's build the App
            AppInstance = builder.Build();

            ApplicationInfo.services = AppInstance.Services;


            // Configure the HTTP request pipeline.






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
            //AppInstance.UseCors(builder =>
            //      builder.AllowAnyOrigin()
            //      .SetIsOriginAllowed((host) => true)
            //      .AllowAnyMethod()
            //      .AllowAnyHeader());

            AppInstance.UseCookiePolicy();
            AppInstance.UseAuthentication();
            AppInstance.UseAuthorization();
            AppInstance.UseSession();
            //AppInstance.MapControllers();
            AppInstance.MapBlazorHub();
            AppInstance.MapFallbackToPage("/_Host");

            AppInstance.Start();
            GetRunningWebServerConfiguration();
            ScheduleAutoLoad();

            AppInstance.WaitForShutdown();
            Log.Information("Application Shutting Down");
            //AppInstance.Run();

        }

        private static void SetupKestrel(WebApplicationBuilder builder)
        {
           
            var _programDbFactory = new AppDatabaseFactory(Configuration);
            var kestrelContext = _programDbFactory.CreateDbContext();


            if (!ApplicationInfo.isUnderIIS && !Debugger.IsAttached)
            {
                var listeningAddress = Configuration.GetValue<string>("ListeningAddress");
                var httpPort = Configuration.GetValue<int>("HTTPPort");
                var httpsPort = Configuration.GetValue<int>("HTTPSPort");
                AppSettings? dbSettings=null;
                X509Certificate2? cert = null;
                try
                {
                    dbSettings = kestrelContext.AppSettings.FirstOrDefault();

                    var certBytes = dbSettings.SSLCertificateCipher.Decrypt<byte[]>();
                    cert = new X509Certificate2(certBytes);

                }
                catch (Exception ex)
                {
                    Loggers.SystemLogger.Error("Error collecting SSL information {@Error}", ex);
                }
                builder.WebHost.UseKestrel(options =>
                {
                    if (listeningAddress == "*")
                    {
                        options.ListenAnyIP(httpPort);
                        if (httpsPort != 0 && dbSettings!=null && cert!=null && cert.HasPrivateKey)
                        {
                            options.ListenAnyIP(httpsPort, configure =>
                            {
                                configure.UseHttps(options=>options.ServerCertificate=cert);
                            });
                        }
                    }

                    else
                    {
                        var ip = IPAddress.Parse(listeningAddress);

                        options.Listen(ip, httpPort);
                        if (httpsPort != 0 && dbSettings != null && cert != null && cert.HasPrivateKey)
                        {
                            options.Listen(ip, httpsPort, configure =>
                            {
                                configure.UseHttps(options => options.ServerCertificate = cert);
                            });
                        }
                    }
                });
            }

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




        public static bool IsDevelopment
        {
            get
            {
                return AppInstance.Environment.IsDevelopment();
            }
        }
    }
}