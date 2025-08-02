using Serilog;
using Serilog.Events;

namespace BLAZAM.Logger
{
    public static class Loggers
    {
        public static string LogPath => _logPath;
        private static string _logPath;
        private static string _applicationVersion;
        public static bool SendToSeqServer { get; set; } = true;
        public static bool? InstallationCompleted { get; set; } = null;
        public static string SeqServerUri { get; set; }
        public static string SecondarySeqServerUri { get; set; }
        public static string InstallationId { get; set; }
        public static string InstallationType { get; set; }
        public static string DatabaseType { get; set; }
        public static string SeqAPIKey { get; set; }

     
        private static ILogger? _requestLogger;
        public static ILogger RequestLogger
        {
            get
            {
                EnsureLogger(ref _requestLogger);
                return _requestLogger;
            }
            private set
            {
                _requestLogger = value;
            }
        }

        private static ILogger? _databaseLogger;
        public static ILogger DatabaseLogger
        {
            get
            {
                EnsureLogger(ref _databaseLogger);
                return _databaseLogger;
            }
            private set
            {
                _databaseLogger = value;
            }
        }

        private static ILogger? _activeDirectoryLogger;
        public static ILogger ActiveDirectoryLogger
        {
            get
            {
                EnsureLogger(ref _activeDirectoryLogger);
                return _activeDirectoryLogger;
            }
            private set
            {
                _activeDirectoryLogger = value;
            }
        }


        private static ILogger? _updateLogger;
        public static ILogger UpdateLogger
        {
            get
            {
                EnsureLogger(ref _updateLogger);
                return _updateLogger;
            }
            private set
            {
                _updateLogger = value;
            }
        }

        private static ILogger? _rulesLogger;
        public static ILogger RulesLogger
        {
            get
            {
                EnsureLogger(ref _rulesLogger);
                return _rulesLogger;
            }
            private set
            {
                _rulesLogger = value;
            }
        }

        private static ILogger? _systemLogger;
        public static ILogger SystemLogger
        {
            get
            {
                EnsureLogger(ref _systemLogger);
                return _systemLogger;
            }
            private set
            {
                _systemLogger = value;
            }
        }

        private static ILogger? _pluginManager;
        public static ILogger PluginLogger
        {
            get
            {
                EnsureLogger(ref _pluginManager);
                return _pluginManager;
            }
            private set
            {
                _pluginManager = value;
            }
        }

   

        public static void SetupLoggers(string logPath, string applicationVersion = "1.0")
        {
            _logPath = logPath;
            _applicationVersion = applicationVersion;
            RequestLogger = SetupLogger(logPath + @"requests\requests.txt");
            DatabaseLogger = SetupLogger(logPath + @"database\db.txt");
            ActiveDirectoryLogger = SetupLogger(logPath + @"activedirectory\activedirectory.txt");
            UpdateLogger = SetupLogger(logPath + @"update\update.txt", RollingInterval.Month);
            PluginLogger = SetupLogger(logPath + @"plugins\plugins.txt", RollingInterval.Month);
            RulesLogger = SetupLogger(logPath + @"rules\rules.txt");

            var systemLoggerBuilder = CreateLogBuilder()
                    .WriteTo.File(logPath + @"system\system.txt",
                    rollingInterval: RollingInterval.Hour,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}",
                    retainedFileTimeLimit: TimeSpan.FromDays(30))
                    .WriteTo.Logger(lc =>
                    {
                        //lc.WriteTo.Console();
                        lc.Filter.ByExcluding(e => e.Level == LogEventLevel.Information).WriteTo.Console();
                    });
            if (SendToSeqServer)
            {
                systemLoggerBuilder.WriteTo.Seq(SeqServerUri, apiKey: SeqAPIKey, restrictedToMinimumLevel: LogEventLevel.Warning);
            }
            Log.Logger = systemLoggerBuilder.CreateLogger();
            SystemLogger = Log.Logger;

            //Serilog.Debugging.SelfLog.Enable(Console.Error);
        }

        internal static LoggerConfiguration CreateLogBuilder()
        {
           var config =  new LoggerConfiguration()
                                .Enrich.FromLogContext()
                               .Enrich.WithMachineName()
                               .Enrich.WithEnvironmentName()
                               .Enrich.WithEnvironmentUserName()
                             .Enrich.WithProperty("Application Name", "Blazam")
                             .Enrich.WithProperty("Installation Type", InstallationType)
                             .Enrich.WithProperty("Installation Id", InstallationId)
                             .Enrich.WithProperty("OS", OperatingSystem.IsWindows() ? "Windows" : OperatingSystem.IsLinux() ? "Linux" : "Unknown")
                             .Enrich.WithProperty("Installation Completed", InstallationCompleted)
                             .Enrich.WithProperty("Database Type", DatabaseType)
                               .Enrich.WithProperty("Application Version", _applicationVersion);
            if (pluginName != null)
            {
                config.Enrich.WithProperty("Plugin Name", pluginName);
                config.Enrich.WithProperty("Plugin Version", pluginVersion);
            }
            return config;
        }

        public static ILogger SetupLogger(string logFilePath, RollingInterval rollingInterval = RollingInterval.Hour)
        {
            var loggerBuilder = CreateLogBuilder()
                .WriteTo.File(logFilePath,
                rollingInterval: rollingInterval,
         outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}",
         retainedFileTimeLimit: TimeSpan.FromDays(30))

                .WriteTo.Logger(lc =>
                {
                    //lc.WriteTo.Console();
                    lc.Filter.ByExcluding(e => e.Level == LogEventLevel.Information).WriteTo.Console();
                });
            if (SendToSeqServer)
            {
                loggerBuilder.WriteTo.Seq(SeqServerUri, apiKey: SeqAPIKey, restrictedToMinimumLevel: LogEventLevel.Warning);
            }

            return loggerBuilder.CreateLogger();
        }

        private static void EnsureLogger(ref ILogger? logger)
        {
            logger ??= SetupTestLogger();
        }

        private static Serilog.ILogger SetupTestLogger()
        {
            var loggerBuilder = CreateLogBuilder()
                .WriteTo.Console();
          

            return loggerBuilder.CreateLogger();
        }

    }
}
