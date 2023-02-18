using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Common
{
    public static class Loggers
    {
        public static ILogger? RequestLogger { get; private set; }
        public static ILogger? DatabaseLogger { get; private set; }
        public static ILogger? ActiveDirectryLogger { get; private set; }
        public static ILogger? UpdateLogger { get; private set; }
        public static ILogger? SystemLogger { get; set; }

        public static void SetupLoggers(string logPath)
        {

            RequestLogger = SetupLogger(logPath+@"requests\requests.txt");
            DatabaseLogger = SetupLogger(logPath+@"database\db.txt");
            ActiveDirectryLogger = SetupLogger(logPath + @"activedirectory\activedirectory.txt");
            UpdateLogger = SetupLogger(logPath + @"update\update.txt",RollingInterval.Day);

            Log.Logger = new LoggerConfiguration()
                    .Enrich.FromLogContext()


                    .WriteTo.File(logPath + @"system\system.txt", rollingInterval: RollingInterval.Hour, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}")
                    .WriteTo.Logger(lc =>
                    {
                        lc.Filter.ByExcluding(e => e.Level == LogEventLevel.Information).WriteTo.Console();
                    })

                    .CreateLogger();
            SystemLogger = Log.Logger;
        }

        private static Serilog.ILogger SetupLogger(string logFilePath,RollingInterval rollingInterval=RollingInterval.Hour)
        {
            return new LoggerConfiguration()
               .Enrich.FromLogContext()
               //.WriteTo.File(WritablePath+@"\logs\log.txt")
               .WriteTo.File(logFilePath,
               rollingInterval: RollingInterval.Hour,
   outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}")

               .CreateLogger();
        }

    }
}
