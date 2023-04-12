
using BLAZAM.Common.Data.Services;
using BLAZAM.Server.Background;
using BLAZAM.Server.Data.Services.Duo;
using BLAZAM.Server.Data.Services.Email;
using BLAZAM.Server.Data.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using MudBlazor.Services;
using System.Globalization;
using MudBlazor;
using BLAZAM.Server.Data;
using BLAZAM.Update.Services;
using BLAZAM.Update;
using BLAZAM.Database.Context;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.ActiveDirectory;
using BLAZAM.Session.Interfaces;
using BLAZAM.Notifications.Services;

namespace BLAZAM.Server
{
    public static class ProgramHelpers
    {
        public static void IntializeProperties(this WebApplicationBuilder builder)
        {
            //Set DebugMode flag from configuration
            Program.InDebugMode = builder.Configuration.GetValue<bool>("DebugMode");
            Program.InDemoMode = builder.Configuration.GetValue<bool>("DemoMode");


            //Set application directories
            Program.RootDirectory = new SystemDirectory(builder.Environment.ContentRootPath);
            Program.TempDirectory = new SystemDirectory(Path.GetTempPath() + "Blazam\\");
            Program.AppDataDirectory = new SystemDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Blazam\\");


            //Store the configuration so other pages/objects can easily access it
            Program.Configuration = builder.Configuration;
        }

        public static void InjectServices(this WebApplicationBuilder builder)
        {

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


            builder.Services.AddSingleton<ApplicationInfo>();


            //Add the httpcontext to services so we can detect the users login status
            builder.Services.AddHttpContextAccessor();

            //Set up authentication and api token authentication
            builder.Services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            builder.Services.AddAuthentication(
                CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(AppAuthenticationStateProvider.ApplyAuthenticationCookieOptions());


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
            builder.Services.AddServerSideBlazor()
                .AddCircuitOptions(options => {
                    options.DetailedErrors = Program.InDebugMode;
                });

            //Inject the database as a service

            DatabaseContextBase.Configuration = builder.Configuration;

            builder.Services.AddSingleton<AppDatabaseFactory>();

            //builder.Services.AddDbContextFactory<DatabaseContext>();

            builder.Services.AddScoped<AppNavigationManager>();


            //Provide an Http client as a service with custom construction via api service class
            builder.Services.AddHttpClient();
            //Also keeping this here for a possible future API, though this would be for internal use
            //builder.Services.AddTransient<ApiService>();
            //builder.Services.AddTransient<IPrincipal>(provider => provider.GetService<IHttpContextAccessor>().HttpContext.User);

            //Provide a way to get the current HTTP userPrincipal as a service
            builder.Services.AddHttpContextAccessor();

            //This probably don't need to be here
            //builder.Services.AddSingleton<WmiFactoryService>();

            //Provide updating as a service, may be a little much for one page using it
            builder.Services.AddSingleton<UpdateService>();

            //Provide the email client as a service
            builder.Services.AddSingleton<EmailService>();


            //Provide a primary Active Directory connection as a service
            //We run this as a singleton so each user connection doesn't have to wait for connection verification to happen
            builder.Services.AddSingleton<IActiveDirectoryContext, ActiveDirectoryContext>();

            //Provide a per-user Active Directory connection as a service
            builder.Services.AddSingleton<IActiveDirectoryContextFactory, ActiveDirectoryContextFactory>();


            //Provide an ApplicationManager as a service
            builder.Services.AddSingleton<ApplicationManager>();

            //Provide a PermissionHandler as a service
            builder.Services.AddSingleton<PermissionApplicator>();
            
            builder.Services.AddSingleton<UserSeederService>();

            //Provide a AuditLogger as a service
            builder.Services.AddScoped<AuditLogger>();




            //Add custom Auth
            builder.Services.AddScoped<AppAuthenticationStateProvider, AppAuthenticationStateProvider>();

            //Add web user application search as a service
            builder.Services.AddScoped<SearchService>();



            builder.Services.AddScoped<ICurrentUserStateService,CurrentUserStateService>();


            builder.Services.AddScoped<UserActiveDirectoryService>();

            //Provide DuoSecurity service
            builder.Services.AddSingleton<IDuoClientProvider, DuoClientProvider>();

            //Provide encyption service
            //There's no benefit to filling memory with identical instances of this, so singleton
            builder.Services.AddSingleton<IEncryptionService, EncryptionService>();

            //Provide database and active directory monitoring service
            //This serivice runs a Timer, and so singleton
            builder.Services.AddSingleton<ConnMonitor>();


            //Provide notification publishing as a service
            builder.Services.AddSingleton<INotificationPublisher,NotificationPublisher>();

            //Provide UserStates as a service
            //This service is a "hack" for Blazor Server not having, in a real sense, sessions
            //It allows data to persist between refreshes/reloading page navigations per logged
            //in user principal
            builder.Services.AddSingleton<IApplicationUserStateService, ApplicationUserStateService>();

            //Provide Automatic Updates as a service
            //This service runs checks every 4 hours for an update and if found, schedules an
            //update at a time of day specified in the database
            builder.Services.AddSingleton<AutoUpdateService>();







            builder.Services.AddMudServices(configuration =>
            {
                configuration.SnackbarConfiguration.HideTransitionDuration = 250;
                configuration.SnackbarConfiguration.ShowTransitionDuration = 250;

            });

            builder.Services.AddMudMarkdownServices();


            builder.Services.AddScoped<AppSnackBarService>();

            builder.Services.AddScoped<AppDialogService>();



            builder.Host.UseWindowsService();
        }
    }
}
