using BLAZAM.Database.Models;
using Microsoft.EntityFrameworkCore;
namespace BLAZAM.Database.Context
{
    public static class DatabaseCache
    {
        private static bool _started;

        private static IAppDatabaseFactory dbContextFactory;

        public static byte[]? AppIcon
        {
            get
            {
                var appIcon = ApplicationSettings?.AppIcon;

                return appIcon;
            }
        }
        public static void Start(IAppDatabaseFactory factory)
        {
            if (!_started)
            {
                _started = true;
                dbContextFactory = factory;
                Task.Run(async () =>
                {
                    while (_started)
                    {
                        await CachingLoop();
                        await Task.Delay(TimeSpan.FromSeconds(20));
                    }
                });

            }
        }
        public static void Stop()
        {
            _started = false;
        }
        private static async Task CachingLoop()
        {

            ActiveDirectorySettings = await UpdateProperty(ActiveDirectorySettings, c => c.ActiveDirectorySettings);

            ApplicationSettings = await UpdateProperty(ApplicationSettings, c => c.AppSettings);

            AuthenticationSettings = await UpdateProperty(AuthenticationSettings, c => c.AuthenticationSettings);

        }


        private static async Task<T?> UpdateProperty<T>(T originalProperty, Func<IDatabaseContext, IQueryable<T>> value)
        {

            using var _context = await dbContextFactory.CreateDbContextAsync();
            try
            {
                var temp = await value.Invoke(_context).FirstOrDefaultAsync();
                return temp;
            }
            catch (Exception)
            {
                // Ignore errors in the cache update, we will just return the original property
            }

            return originalProperty;

        }



        public static ADSettings? ActiveDirectorySettings
        {
            get;
            set;
        }
        public static AppSettings? ApplicationSettings
        {
            get;
            set;
        }
        public static AuthenticationSettings? AuthenticationSettings { get; set; }




    }
}