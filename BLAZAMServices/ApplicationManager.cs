using BLAZAM.Logger;
using Microsoft.Extensions.Hosting;

namespace BLAZAM.Services
{
    /// <summary>
    /// Manages application-level operations, such as restarting the application.
    /// </summary>
    public class ApplicationManager
    {
        private IHostApplicationLifetime ApplicationLifetime { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationManager"/> class.
        /// </summary>
        /// <param name="applicationLifetime">The .NET host application lifetime service.</param>
        /// <exception cref="ArgumentNullException">If applicationLifetime is null.</exception>
        public ApplicationManager(IHostApplicationLifetime applicationLifetime)
        {
            ArgumentNullException.ThrowIfNull(applicationLifetime);

            ApplicationLifetime = applicationLifetime;
        }

        /// <summary>
        /// Initiates a graceful shutdown of the application by calling StopApplication on the IHostApplicationLifetime.
        /// </summary>
        public void Restart()
        {
            Loggers.SystemLogger.Information("ApplicationManager: Application restart requested via StopApplication().");
            ApplicationLifetime.StopApplication();
        }
    }
}
