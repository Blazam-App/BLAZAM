using BLAZAM.Logger; // Added
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System; // Added

namespace BLAZAM.Notifications.Services
{
    /// <summary>
    /// Provides a wrapper around MudBlazor's <see cref="ISnackbar"/> service for displaying application-styled snackbar notifications, with added logging capabilities.
    /// </summary>
    public class AppSnackBarService
    {
        private ISnackbar _snackbar { get; set; }

        /// <summary>Initializes a new instance of the <see cref="AppSnackBarService"/> class.</summary> 
        /// <param name="snackbar">The MudBlazor <see cref="ISnackbar"/> service to be wrapped.</param> 
        /// <exception cref="ArgumentNullException">Thrown if snackbar service is null.</exception>
        public AppSnackBarService(ISnackbar snackbar)
        {
            ArgumentNullException.ThrowIfNull(snackbar);

           
            _snackbar = snackbar;
        }

        /// <summary>Displays an error snackbar notification with the specified string message.</summary> 
        /// <param name="message">The message to display.</param>
        public void Error(string message)
        {
            Loggers.SystemLogger.Debug("AppSnackBarService.Error (string): Displaying error snackbar with message: {Message}", message);
            _snackbar.Add(message, Severity.Error, configure => configure.RequireInteraction = true);
        }

        /// <summary>Displays an error snackbar notification with the specified RenderFragment content.</summary> 
        /// <param name="message">The RenderFragment content to display.</param>
        public void Error(RenderFragment message)
        {
            Loggers.SystemLogger.Debug("AppSnackBarService.Error (RenderFragment): Displaying error snackbar with RenderFragment content.");
            _snackbar.Add(message, Severity.Error, configure => configure.RequireInteraction = true);
        }

        /// <summary>Displays an info snackbar notification with the specified string message.</summary> 
        /// <param name="message">The message to display.</param>
        public void Info(string message)
        {
            Loggers.SystemLogger.Debug("AppSnackBarService.Info (string): Displaying info snackbar with message: {Message}", message);
            _snackbar.Add(message, Severity.Info);
        }

        /// <summary>Displays an info snackbar notification with the specified RenderFragment content.</summary> 
        /// <param name="message">The RenderFragment content to display.</param>
        public void Info(RenderFragment message)
        {
            Loggers.SystemLogger.Debug("AppSnackBarService.Info (RenderFragment): Displaying info snackbar with RenderFragment content.");
            _snackbar.Add(message, Severity.Info);
        }

        /// <summary>Displays a warning snackbar notification with the specified string message.</summary> 
        /// <param name="message">The message to display.</param>
        public void Warning(string message)
        {
            Loggers.SystemLogger.Debug("AppSnackBarService.Warning (string): Displaying warning snackbar with message: {Message}", message);
            _snackbar.Add(message, Severity.Warning);
        }

        /// <summary>Displays a warning snackbar notification with the specified RenderFragment content.</summary> 
        /// <param name="message">The RenderFragment content to display.</param>
        public void Warning(RenderFragment message)
        {
            Loggers.SystemLogger.Debug("AppSnackBarService.Warning (RenderFragment): Displaying warning snackbar with RenderFragment content.");
            _snackbar.Add(message, Severity.Warning);
        }

        /// <summary>Displays a success snackbar notification with the specified string message.</summary> 
        /// <param name="message">The message to display.</param>
        public void Success(string message)
        {
            Loggers.SystemLogger.Debug("AppSnackBarService.Success (string): Displaying success snackbar with message: {Message}", message);
            _snackbar.Add(message, Severity.Success);
        }

        /// <summary>Displays a success snackbar notification with the specified RenderFragment content.</summary> 
        /// <param name="message">The RenderFragment content to display.</param>
        public void Success(RenderFragment message)
        {
            Loggers.SystemLogger.Debug("AppSnackBarService.Success (RenderFragment): Displaying success snackbar with RenderFragment content.");
            _snackbar.Add(message, Severity.Success);
        }
    }
}
