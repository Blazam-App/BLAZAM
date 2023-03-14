
using BLAZAM.Database.Models.Database;
using Microsoft.EntityFrameworkCore;
using Serilog;
namespace BLAZAM.Database.Data.Database
{
    public class DatabaseCache : IDisposable
    {
        private static bool _started;

        private static IDbContextFactory<DatabaseContext> dbContextFactory;
        private static ILogger _logger;

        public static byte[] AppIcon
        {
            get
            {
                var appIcon = ApplicationSettings?.AppIcon;
               
                return appIcon;
            }
        }
        public static void Start(IDbContextFactory<DatabaseContext> factory, ILogger logger)
        {
            _logger = logger;
            if (!_started)
            {
                _logger.Information("Starting Database Cache");
                _started = true;
                dbContextFactory = factory;
                Task.Run(() =>
                {
                    while (_started)
                    {
                        CachingLoop();
                        Task.Delay(TimeSpan.FromSeconds(20)).Wait();
                    }
                });

            }
        }
        public static void Stop()
        {
            _logger?.Information("Stoopping Database Cache");

            _started = false;
        }
        private static void CachingLoop()
        {
            Task.Run(async () =>
            {
                ActiveDirectorySettings = await UpdateProperty<ADSettings>(ActiveDirectorySettings, c => c.ActiveDirectorySettings);

            });
            Task.Run(async () =>
            {
                ApplicationSettings = await UpdateProperty<AppSettings>(ApplicationSettings, c => c.AppSettings);



            });
            Task.Run(async () =>
            {
                EmailSettings = await UpdateProperty<EmailSettings>(EmailSettings, c => c.EmailSettings);

            });
            Task.Run(async () =>
            {
                AuthenticationSettings = await UpdateProperty<AuthenticationSettings>(AuthenticationSettings, c => c.AuthenticationSettings);

            });


        }


        private static async Task<T> UpdateProperty<T>(T originalProperty, Func<DatabaseContext, IQueryable<T>> value)
        {

            //Console.WriteLine("Updating "+typeof(T).Name);

            using var _context = await dbContextFactory.CreateDbContextAsync();
            try
            {
                var temp = await value.Invoke(_context).FirstOrDefaultAsync();
                //Console.WriteLine("Finished " + typeof(T).Name);

                return temp;
            }
            catch (Exception)
            {

            }
            //Console.WriteLine("Finished " + typeof(T).Name);

            return originalProperty;

        }

        public void Dispose()
        {
            _started = false;

        }

        public static ADSettings ActiveDirectorySettings
        {
            get;
            set;
        }
        public static AppSettings ApplicationSettings
        {
            get;
            set;
        }
        public static EmailSettings EmailSettings { get; set; }
        public static AuthenticationSettings AuthenticationSettings { get; set; }




    }
}