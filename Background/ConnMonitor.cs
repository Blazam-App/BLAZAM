using BLAZAM.Common.Data.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data.Database;
using BLAZAM.Server.Pages.Error;
using Microsoft.EntityFrameworkCore;

namespace BLAZAM.Server.Background
{
    public class ConnMonitor
    {

        public DatabaseMonitor DatabaseMonitor;
        public DirectoryMonitor DirectoryMonitor;
        private IDbContextFactory<DatabaseContext> _factory;
        private DatabaseContext _context;
        public AppEvent<ConnectionState>? OnAppReadyChanged { get; set; }
        public AppEvent<ConnectionState>? OnDirectoryConnectionChanged { get; set; }

        public bool RedirectToHttps { get; set; }
        public ConnectionState? DatabaseConnected { get => DatabaseMonitor.Connected; }
        public ConnectionState? DirectoryConnected { get => DirectoryMonitor.Connected; }
        public ConnectionState AppReady { get; protected set; } = ConnectionState.Connecting;
        public bool DatabaseUpdatePending { get; private set; }

        private Timer? _timer;
        bool _monitoring;

        public ConnMonitor(IDbContextFactory<DatabaseContext> DbFactory, IActiveDirectory directory)
        {
            _factory = DbFactory;
            _context = DbFactory.CreateDbContext();
            DatabaseMonitor = new DatabaseMonitor(_context);
            DirectoryMonitor = new DirectoryMonitor(DbFactory, directory);
            DatabaseMonitor.OnConnectedChanged += ((ConnectionState newStatus) =>
            {
                if (AppReady != newStatus)
                {
                    OnAppReadyChanged?.Invoke(newStatus);
                    AppReady = newStatus;
                }


            });
            DirectoryMonitor.OnConnectedChanged += ((ConnectionState newStatus) =>
            {
                OnDirectoryConnectionChanged?.Invoke(newStatus);
            });
            MonitorDatabaseValues();
        }

        private void MonitorDatabaseValues()
        {
            if (_monitoring == false)
            {
                _monitoring = true;

                // Create a timer with a 20 second interval
                _timer = new Timer(UpdateCache, null, 0, 20000);


            }
        }


        private void UpdateCache(object? state)
        {
            Task.Run(() =>
            {
                using (var _context = _factory.CreateDbContext())
                {
                    try
                    {
                        RedirectToHttps = _context.AppSettings.First().ForceHTTPS;
                       
                    }
                    catch (Exception)
                    {

                    }
                    try
                    {
                        var temp = _context.Database.GetPendingMigrations();
                        if (temp != null && temp.Count() > 0)
                            DatabaseUpdatePending = true;
                        else
                            DatabaseUpdatePending = false;

                    }
                    catch (Exception)
                    {

                    }
                  

                }
            });

        }
    }
}
