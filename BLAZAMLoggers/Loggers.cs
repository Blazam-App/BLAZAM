using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Logger
{
    public static class Loggers
    {
        public static string LogPath => _logPath;
        private static string _logPath;
        private static string _applicationVersion;
        public static bool SendToSeqServer { get; set; } = true;
        public static string SeqServerUri { get; set; }
        public static string InstallationId { get; set; }
        public static string SeqAPIKey { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public static ILogger RequestLogger { get; private set; }
        public static ILogger DatabaseLogger { get; private set; }
        public static ILogger ActiveDirectoryLogger { get; private set; }
        public static ILogger UpdateLogger { get; private set; }
        public static ILogger SystemLogger { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public static void SetupLoggers(string logPath, string applicationVersion = "1.0")
        {
            _logPath = logPath;
            _applicationVersion = applicationVersion;
            RequestLogger = SetupLogger(logPath + @"requests\requests.txt");
            DatabaseLogger = SetupLogger(logPath + @"database\db.txt");
            ActiveDirectoryLogger = SetupLogger(logPath + @"activedirectory\activedirectory.txt");
            UpdateLogger = SetupLogger(logPath + @"update\update.txt", RollingInterval.Month);

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

        private static LoggerConfiguration CreateLogBuilder()
        {
            return new LoggerConfiguration()
                                .Enrich.FromLogContext()
                               .Enrich.WithMachineName()
                               .Enrich.WithEnvironmentName()
                               .Enrich.WithEnvironmentUserName()
                             .Enrich.WithProperty("Application Name", "Blazam")
                             .Enrich.WithProperty("Installation Id", InstallationId)
                               .Enrich.WithProperty("Application Version", _applicationVersion);
        }

        private static Serilog.ILogger SetupLogger(string logFilePath, RollingInterval rollingInterval = RollingInterval.Hour)
        {
            var loggerBuilder =CreateLogBuilder()
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
