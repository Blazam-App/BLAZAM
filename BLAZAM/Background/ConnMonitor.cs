using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using BLAZAM.Common.Data.Services;
using BLAZAM.Database.Context;
using BLAZAM.Server.Pages.Error;
using Microsoft.EntityFrameworkCore;

namespace BLAZAM.Server.Background
{
    public class ConnMonitor
    {

        public readonly DatabaseMonitor DatabaseMonitor;
        public readonly DirectoryMonitor DirectoryMonitor;
        private readonly IEncryptionService _encryption;
        private readonly AppDatabaseFactory _factory;
        private readonly IDatabaseContext _context;
        public AppEvent<ServiceConnectionState>? OnAppReadyChanged { get; set; }
        public AppEvent<ServiceConnectionState>? OnDirectoryConnectionChanged { get; set; }

        public bool RedirectToHttps { get; set; }
        public ServiceConnectionState? DatabaseConnected { get => DatabaseMonitor.Status; }
        public ServiceConnectionState? DirectoryConnected { get => DirectoryMonitor.Status; }
        private bool _failedMigration;
        /// <summary>
        /// Indicated whether the application is ready to serve users.
        /// </summary>
        /// 
        public ServiceConnectionState AppReady
        {
            get { return _failedMigration==true?ServiceConnectionState.Down:_appReady; } protected set
            {
                if (_appReady == value) return;
                _appReady = value;
                OnAppReadyChanged?.Invoke(value);


            }
        }
        public bool DatabaseUpdatePending { get; private set; }

        private Timer? _timer;
        bool _monitoring;
        private ServiceConnectionState _appReady = ServiceConnectionState.Connecting;

        public ConnMonitor(AppDatabaseFactory DbFactory, IActiveDirectoryContext directory, IEncryptionService encryption)
        {
            _encryption = encryption;
            _factory = DbFactory;
            _context = DbFactory.CreateDbContext();
            DatabaseMonitor = new DatabaseMonitor(_context);
            DirectoryMonitor = new DirectoryMonitor(directory);
            DatabaseMonitor.OnConnectedChanged += ((ServiceConnectionState newStatus) =>
            {
                if (_encryption.Status == ServiceConnectionState.Down)
                {
                    Oops.ErrorMessage = "EncryptionKey missing or invalid in appsettings.json";
                    AppReady = ServiceConnectionState.Down;
                    return;
                }
                if (AppReady != newStatus)
                {
                    OnAppReadyChanged?.Invoke(newStatus);
                    AppReady = newStatus;
                    if(newStatus== ServiceConnectionState.Down && DatabaseContextBase.DownReason!=null)
                    {
                        Oops.ErrorMessage = DatabaseContextBase.DownReason.GetType().FullName;
                        Oops.HelpMessage = DatabaseContextBase.DownReason.Message;
                    }
                }


            });
            DirectoryMonitor.OnConnectedChanged += ((ServiceConnectionState newStatus) =>
            {
                OnDirectoryConnectionChanged?.Invoke(newStatus);
            });

            DatabaseContextBase.OnMigrationFailed += (() => {
                Oops.Exception = DatabaseContextBase.DownReason;
                _failedMigration = true;
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
