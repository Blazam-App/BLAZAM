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
        private static string LogPath;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public static ILogger RequestLogger { get; private set; }
        public static ILogger DatabaseLogger { get; private set; }
        public static ILogger ActiveDirectryLogger { get; private set; }
        public static ILogger UpdateLogger { get; private set; }
        public static ILogger SystemLogger { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public static void SetupLoggers(string logPath)
        {
            LogPath = logPath;
            RequestLogger = SetupLogger(logPath+@"requests\requests.txt");
            DatabaseLogger = SetupLogger(logPath+@"database\db.txt");
            ActiveDirectryLogger = SetupLogger(logPath + @"activedirectory\activedirectory.txt");
            UpdateLogger = SetupLogger(logPath + @"update\update.txt",RollingInterval.Month);

            Log.Logger = new LoggerConfiguration()
                    .Enrich.FromLogContext()


                    .WriteTo.File(logPath + @"system\system.txt",
                    rollingInterval: RollingInterval.Hour,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}",
                    retainedFileTimeLimit: TimeSpan.FromDays(30))
                    .WriteTo.Logger(lc =>
                    {
                        //lc.WriteTo.Console();
                        lc.Filter.ByExcluding(e => e.Level == LogEventLevel.Information).WriteTo.Console();
                    })
                    .WriteTo.Seq("http://logs.blazam.org:5341", apiKey: "8TeLknA8XBk5ybamT5m9", restrictedToMinimumLevel: LogEventLevel.Warning)
                    .CreateLogger();
            SystemLogger = Log.Logger;

            //Serilog.Debugging.SelfLog.Enable(Console.Error);
        }

        private static Serilog.ILogger SetupLogger(string logFilePath,RollingInterval rollingInterval=RollingInterval.Hour)
        {
            return new LoggerConfiguration()
               .Enrich.FromLogContext()
               //.WriteTo.File(WritablePath+@"\logs\log.txt")
               .WriteTo.File(logFilePath,
               rollingInterval: RollingInterval.Hour,
   outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}",
   retainedFileTimeLimit:TimeSpan.FromDays(30))

               .WriteTo.Logger(lc =>
               {
                   //lc.WriteTo.Console();
                   lc.Filter.ByExcluding(e => e.Level == LogEventLevel.Information).WriteTo.Console();
               })
               .WriteTo.Seq("http://logs.blazam.org:5341",apiKey: "8TeLknA8XBk5ybamT5m9", restrictedToMinimumLevel: LogEventLevel.Warning)

               .CreateLogger();
        }
      
    }
}
