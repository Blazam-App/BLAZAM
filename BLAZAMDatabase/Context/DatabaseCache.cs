using BLAZAM.Database.Models;
using Microsoft.EntityFrameworkCore;
namespace BLAZAM.Database.Context
{
    public class DatabaseCache : IDisposable
    {
        private static bool _started;

        private static IAppDatabaseFactory dbContextFactory;

        public static byte[] AppIcon
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
            _started = false;
        }
        private static void CachingLoop()
        {
            Task.Run(async () =>
            {
                ActiveDirectorySettings = await UpdateProperty(ActiveDirectorySettings, c => c.ActiveDirectorySettings);

            });
            Task.Run(async () =>
            {
                ApplicationSettings = await UpdateProperty(ApplicationSettings, c => c.AppSettings);



            });
            Task.Run(async () =>
            {
                EmailSettings = await UpdateProperty(EmailSettings, c => c.EmailSettings);

            });
            Task.Run(async () =>
            {
                AuthenticationSettings = await UpdateProperty(AuthenticationSettings, c => c.AuthenticationSettings);

            });


        }


        private static async Task<T> UpdateProperty<T>(T originalProperty, Func<IDatabaseContext, IQueryable<T>> value)
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