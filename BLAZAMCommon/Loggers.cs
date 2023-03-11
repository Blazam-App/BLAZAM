using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Common
{
    public static class Loggers
    {
        private static string LogPath;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public static ILogger RequestLogger { get; private set; }
        public static ILogger DatabaseLogger { get; private set; }
        public static ILogger ActiveDirectryLogger { get; private set; }
        public static ILogger? UpdateLogger { get; private set; }
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
   outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}",
   retainedFileTimeLimit:TimeSpan.FromDays(30))

               .WriteTo.Logger(lc =>
               {
                   //lc.WriteTo.Console();
                   lc.Filter.ByExcluding(e => e.Level == LogEventLevel.Information).WriteTo.Console();
               })

               .CreateLogger();
        }

        public static ZipArchive GenerateZip()
        {
            MemoryStream memoryStream = new MemoryStream();
            ZipArchive zip = new ZipArchive(memoryStream,ZipArchiveMode.Create,true);
            // Recursively add files and subdirectories to the zip archive
            zip.AddToZip(new SystemDirectory(LogPath));
            return zip;
        }
    }
}
