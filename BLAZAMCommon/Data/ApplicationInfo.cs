using BLAZAM.FileSystem;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;

namespace BLAZAM.Common.Data

{
    public class ApplicationInfo
    {

        /// <summary>
        /// The running Blazam version
        /// </summary>
        public static ApplicationVersion runningVersion;

        /// <summary>
        /// The process of the running application
        /// </summary>
        public static Process runningProcess;

        /// <summary>
        /// The root directory of the running web application
        /// </summary>
        /// <returns>
        /// eg: C:\inetpub\blazam\
        /// </returns>
        /// 
        public static SystemDirectory applicationRoot;

        /// <summary>
        /// The temporary file directry
        /// </summary>
        /// <returns>
        /// eg: C:\Users\user\appdata\temp\
        /// </returns>
        public static SystemDirectory tempDirectory;

        /// <summary>
        /// A collection of active listening address's with port
        /// </summary>
        /// <returns>
        /// A list of address strings eg: {"https://localhost:7900/","http://localhost:5900/"}
        /// </returns>
        public static IEnumerable<string> listeningAddresses=new List<string>();
        public static bool inDebugMode;
        public static bool inDemoMode;
        public static IServiceProvider services;
        public static SecurityKey tokenKey;


        /// <summary>
        /// The running Blazam version
        /// </summary>
        public ApplicationVersion RunningVersion { get => runningVersion; set => runningVersion = value; }

        /// <summary>
        /// The process of the running application
        /// </summary>
        public Process RunningProcess { get => runningProcess; set => runningProcess = value; }

        /// <summary>
        /// The root directory of the running web application
        /// </summary>
        /// <returns>
        /// eg: C:\inetpub\blazam\
        /// </returns>
        /// 
        public SystemDirectory ApplicationRoot { get => applicationRoot; set => applicationRoot = value; }

        /// <summary>
        /// The temporary file directry
        /// </summary>
        /// <returns>
        /// eg: C:\Users\user\appdata\temp\
        /// </returns>
        public SystemDirectory TempDirectory { get => tempDirectory; set => tempDirectory = value; }
        public IServiceProvider Services { get => services; set => services = value; }

        /// <summary>
        /// A collection of active listening address's with port
        /// </summary>
        /// <returns>
        /// A list of address strings eg: {"https://localhost:7900/","http://localhost:5900/"}
        /// </returns>
        public IEnumerable<string> ListeningAddresses { get => listeningAddresses; set => listeningAddresses = value; }
        public bool InstallationCompleted { get => installationCompleted; set => installationCompleted = value; }
        public bool InDebugMode { get => inDebugMode; set => inDebugMode = value; }
        public bool InDemoMode { get => inDemoMode; set => inDemoMode = value; }

        /// <summary>
        /// Indicates the Installation status
        /// </summary>
        public static bool installationCompleted
        {
            get;set;
        }

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