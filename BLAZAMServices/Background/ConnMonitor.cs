using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using BLAZAM.Common.Data.Services;
using BLAZAM.Database.Context;
using BLAZAM.Database.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace BLAZAM.Services.Background
{
    public class ConnMonitor
    {

        public readonly DatabaseMonitor DatabaseMonitor;
        public readonly DirectoryMonitor DirectoryMonitor;
        private readonly IEncryptionService _encryption;
        private readonly IAppDatabaseFactory _factory;
        private readonly IDatabaseContext _context;
        /// <summary>
        /// Called when the database can connect and application settings have been loaded
        /// </summary>
        public AppEvent<ServiceConnectionState>? OnAppReadyChanged { get; set; }
        /// <summary>
        /// Called when the Active Directory connection changes
        /// </summary>
        public AppEvent<ServiceConnectionState>? OnDirectoryConnectionChanged { get; set; }


        //public bool RedirectToHttps { get; set; }
        public ServiceConnectionState? DatabaseConnectionStatus { get => DatabaseMonitor.Status; }
        public ServiceConnectionState? DirectoryConnectionStatus { get => DirectoryMonitor.Status; }

        /// <summary>
        /// Indicated whether the application is ready to serve users.
        /// </summary>
        /// 
        public ServiceConnectionState AppReady
        {
            get { return AppDatabaseFactory.FatalError != null ? ServiceConnectionState.Down : _appReady; }
            protected set
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

        public ConnMonitor(IAppDatabaseFactory DbFactory, IActiveDirectoryContext directory, IEncryptionService encryption)
        {
            _encryption = encryption;
            _factory = DbFactory;
            _context = DbFactory.CreateDbContext();
            DatabaseMonitor = new DatabaseMonitor(_context);
            DirectoryMonitor = new DirectoryMonitor(directory);
            DatabaseMonitor.OnConnectedChanged += (newStatus) =>
            {
                //TODO Separate Oops logic from razor page
                if (_encryption.Status == ServiceConnectionState.Down)
                {
                    //Oops.ErrorMessage = "EncryptionKey missing or invalid in appsettings.json";
                    AppReady = ServiceConnectionState.Down;
                    return;
                }
                if (AppReady != newStatus)
                {
                    OnAppReadyChanged?.Invoke(newStatus);
                    AppReady = newStatus;
                    if (newStatus == ServiceConnectionState.Down && DatabaseContextBase.DownReason != null)
                    {
                       // Oops.ErrorMessage = DatabaseContextBase.DownReason.GetType().FullName;
                        //Oops.HelpMessage = DatabaseContextBase.DownReason.Message;
                    }
                }


            };
            DirectoryMonitor.OnConnectedChanged += (newStatus) =>
            {
                OnDirectoryConnectionChanged?.Invoke(newStatus);
            };

            
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
