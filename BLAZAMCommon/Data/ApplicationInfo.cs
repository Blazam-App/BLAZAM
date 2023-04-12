using BLAZAM.FileSystem;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace BLAZAM.Common.Data

{
    public class ApplicationInfo
    {
        public static ApplicationVersion runningVersion;
        public static Process runningProcess;
        public static SystemDirectory applicationRoot;
        public static SystemDirectory tempDirectory;
        public static IEnumerable<string> listeningAddresses;
        public static bool inDebugMode;
        public static bool inDemoMode;
        public static IServiceProvider services;
        public static bool installationCompleted;

        public ApplicationVersion RunningVersion { get => runningVersion; set => runningVersion = value; }
        public Process RunningProcess { get => runningProcess; set => runningProcess = value; }
        public SystemDirectory ApplicationRoot { get => applicationRoot; set => applicationRoot = value; }
        public SystemDirectory TempDirectory { get => tempDirectory; set => tempDirectory = value; }
        public IServiceProvider Services { get => services; set => services = value; }
        public IEnumerable<string> ListeningAddresses { get => listeningAddresses; set => listeningAddresses = value; }
        public bool InstallationCompleted { get => installationCompleted; set => installationCompleted = value; }
        public bool InDebugMode { get => inDebugMode; set => inDebugMode = value; }
        public bool InDemoMode { get => inDemoMode; set => inDemoMode = value; }


        public ApplicationInfo()
        {

        }
        public ApplicationInfo(WebApplicationBuilder builder)
        {
            RunningProcess = Process.GetCurrentProcess();
            ApplicationRoot = new SystemDirectory(builder.Environment.ContentRootPath);
            TempDirectory = new SystemDirectory(Path.GetTempPath() + "Blazam\\");
            //AppDataDirectory = new SystemDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Blazam\\");
        }
    }
}