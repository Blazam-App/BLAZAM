using BLAZAM.Database.Models.User;
using BLAZAM.Logger;

namespace BLAZAM.Notifications.Services
{
    /// <summary>
    /// A static service for broadcasting snackbar messages to all connected application instances (circuits) via delegate events.
    /// Intended for system-wide, immediate UI feedback.
    /// </summary>
    public static class SnackbarBroadcastService
    {
        /// <summary>
        /// Event triggered when an informational message is broadcast. Components can subscribe to this to display the snackbar.
        /// </summary>
        public static AppDelegate<NotificationMessage>? OnInfoBroadcast { get; set; }

        /// <summary>
        /// Event triggered when a success message is broadcast. Components can subscribe to this to display the snackbar.
        /// </summary>
        public static AppDelegate<NotificationMessage>? OnSuccessBroadcast { get; set; }

        /// <summary>
        /// Event triggered when an error message is broadcast. Components can subscribe to this to display the snackbar.
        /// </summary>
        public static AppDelegate<NotificationMessage>? OnErrorBroadcast { get; set; }

        /// <summary>
        /// Event triggered when a warning message is broadcast. Components can subscribe to this to display the snackbar.
        /// </summary>
        public static AppDelegate<NotificationMessage>? OnWarningBroadcast { get; set; }

        /// <summary>
        /// Broadcasts an informational snackbar message to all subscribed listeners.
        /// </summary>
        /// <param name="message">The message content.</param>
        /// <param name="title">Optional title for the snackbar.</param>
        public static void Info(string message, string? title = null)
        {
            Loggers.SystemLogger.Debug("SnackbarBroadcastService.Info: Broadcasting info message. Title: '{Title}', Message: '{Message}'.", title, message);
            OnInfoBroadcast?.Invoke(new NotificationMessage { Title = title, Message = message });
        }

        /// <summary>
        /// Broadcasts a success snackbar message to all subscribed listeners.
        /// </summary>
        /// <param name="message">The message content.</param>
        /// <param name="title">Optional title for the snackbar.</param>
        public static void Success(string message, string? title = null)
        {
            Loggers.SystemLogger.Debug("SnackbarBroadcastService.Success: Broadcasting success message. Title: '{Title}', Message: '{Message}'.", title, message);
            OnSuccessBroadcast?.Invoke(new NotificationMessage { Title = title, Message = message });
        }

        /// <summary>
        /// Broadcasts an error snackbar message to all subscribed listeners.
        /// </summary>
        /// <param name="message">The message content.</param>
        /// <param name="title">Optional title for the snackbar.</param>
        public static void Error(string message, string? title = null)
        {
            Loggers.SystemLogger.Debug("SnackbarBroadcastService.Error: Broadcasting error message. Title: '{Title}', Message: '{Message}'.", title, message);
            OnErrorBroadcast?.Invoke(new NotificationMessage { Title = title, Message = message });
        }

        /// <summary>
        /// Broadcasts a warning snackbar message to all subscribed listeners.
        /// </summary>
        /// <param name="message">The message content.</param>
        /// <param name="title">Optional title for the snackbar.</param>
        public static void Warning(string message, string? title = null)
        {
            Loggers.SystemLogger.Debug("SnackbarBroadcastService.Warning: Broadcasting warning message. Title: '{Title}', Message: '{Message}'.", title, message);
            OnWarningBroadcast?.Invoke(new NotificationMessage { Title = title, Message = message });
        }
    }
}
