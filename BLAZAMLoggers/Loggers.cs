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
        public static string InstallationId { get; set; }
        public static string InstallationType { get; set; }
        public static string DatabaseType { get; set; }
        public static string SeqAPIKey { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public static ILogger RequestLogger { get; private set; }
        public static ILogger DatabaseLogger { get; private set; }
        public static ILogger ActiveDirectoryLogger { get; private set; }
        public static ILogger UpdateLogger { get; private set; }
        public static ILogger RulesLogger { get; private set; }
        public static ILogger SystemLogger { get; set; }
        public static ILogger AspNetLogger{ get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public static void SetupLoggers(string logPath, string applicationVersion = "1.0")
        {
            _logPath = logPath;
            _applicationVersion = applicationVersion;
            RequestLogger = SetupLogger(logPath + $"requests{Path.DirectorySeparatorChar}requests.txt");
            DatabaseLogger = SetupLogger(logPath + $"database{Path.DirectorySeparatorChar}db.txt");
            ActiveDirectoryLogger = SetupLogger(logPath + $"activedirectory{Path.DirectorySeparatorChar}activedirectory.txt");
            UpdateLogger = SetupLogger(logPath + $"update{Path.DirectorySeparatorChar}update.txt", RollingInterval.Month);
            RulesLogger = SetupLogger(logPath + $"rules{Path.DirectorySeparatorChar}rules.txt");
            AspNetLogger = SetupLogger(logPath + $"aspnet{Path.DirectorySeparatorChar}aspnet.txt");
            SystemLogger = SetupLogger(logPath + $"system{Path.DirectorySeparatorChar}system.txt");

          
           
            Log.Logger = AspNetLogger;

            //Serilog.Debugging.SelfLog.Enable(Console.Error);
        }

        private static LoggerConfiguration CreateLogBuilder()
        {
            return new LoggerConfiguration()
                                .Enrich.FromLogContext()
                               .Enrich.WithMachineName()
                               .Enrich.WithEnvironmentName()
                               .Enrich.WithEnvironmentUserName()
                             .Enrich.WithProperty("Application Name", "Blazam")
                             .Enrich.WithProperty("Installation Type", InstallationType)
                             .Enrich.WithProperty("Installation Id", InstallationId)
                             .Enrich.WithProperty("Installation Completed", InstallationCompleted)
                             .Enrich.WithProperty("Database Type", DatabaseType)
                               .Enrich.WithProperty("Application Version", _applicationVersion);
        }

        private static Serilog.ILogger SetupLogger(string logFilePath, RollingInterval rollingInterval = RollingInterval.Hour)
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

    }
}
