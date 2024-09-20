using BLAZAM.FileSystem;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;

namespace BLAZAM.Common.Data

{

    /// <summary>
    /// A store  of base configuration,process, and directory defaults for the
    /// application
    /// </summary>
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
        public static IEnumerable<string> listeningAddresses = new List<string>();

        /// <summary>
        /// A static access to <see cref="InDebugMode"/>
        /// </summary>
        public static bool inDebugMode;

        /// <summary>
        /// A static access to <see cref="InDemoMode"/>
        /// </summary>
        public static bool inDemoMode;
        /// <summary>
        /// A static access to <see cref="InstallationId"/>
        /// </summary>
        public static Guid installationId = Guid.Empty;

        /// <summary>
        /// Indicates whether Blazam is running under IIS or as a service
        /// </summary>
        public static bool isUnderIIS => runningProcess.ProcessName.Contains("w3wp") || runningProcess.ProcessName.Contains("iisexpress");

        /// <summary>
        /// A local store of the .Net web application Services
        /// </summary>
        public static IServiceProvider services;

        [Obsolete("Not using any local REST API")]
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

        /// <summary>
        /// A collection of active listening address's with port
        /// </summary>
        /// <returns>
        /// A list of address strings eg: {"https://localhost:7900/","http://localhost:5900/"}
        /// </returns>
        public IEnumerable<string> ListeningAddresses { get => listeningAddresses; set => listeningAddresses = value; }

        /// <summary>
        /// Indicates whether installation wizard has been completed
        /// </summary>
        /// <remarks>
        /// This relies on a database connection being functional
        /// </remarks>
        public bool InstallationCompleted { get => installationCompleted; set => installationCompleted = value; }

        /// <summary>
        /// Indicates whether Blazam is in Debug Mode
        /// </summary>
        public bool InDebugMode { get => inDebugMode; set => inDebugMode = value; }

        /// <summary>
        /// Indicates whether Blazam is in Demo Mode
        /// </summary>
        /// <remarks>
        /// Used for public dev demo site
        /// </remarks>
        public bool InDemoMode { get => inDemoMode; set => inDemoMode = value; }

        /// <summary>
        /// Indicates whether Blazam is running under IIS or as a service
        /// </summary>
        public bool IsUnderIIS { get => isUnderIIS; }

        /// <summary>
        /// Indicates the Installation status
        /// </summary>
        public static bool installationCompleted
        {
            get; set;
        }
        public Guid InstallationId { get => installationId; set => installationId = value; }

        /// <summary>
        /// Use only for UnitTests
        /// </summary>
        public ApplicationInfo()
        {

        }

        /// <summary>
        /// Creates a new ApplicationInfo object for the running application
        /// </summary>
        /// <param name="builder"></param>
        public ApplicationInfo(WebApplicationBuilder builder)
        {
            RunningProcess = Process.GetCurrentProcess();
            ApplicationRoot = new SystemDirectory(builder.Environment.ContentRootPath);
            TempDirectory = new SystemDirectory(Path.GetTempPath() + "Blazam\\");
            //AppDataDirectory = new SystemDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Blazam\\");
        }
    }
}