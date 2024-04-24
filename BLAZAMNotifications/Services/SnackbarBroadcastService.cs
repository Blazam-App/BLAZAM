
using BLAZAM.Database.Models.User;

namespace BLAZAM.Notifications.Services
{
    /// <summary>
    /// Use this service to send snackbar messages to all currrently 
    /// logged in users.
    /// </summary>
    /// <remarks>
    /// Triggers each circuit's <see cref="AppSnackBarService"/>
    /// </remarks>
    public static class SnackbarBroadcastService
    {

        public static AppEvent<NotificationMessage>? OnInfoBroadcast { get; set; }

        public static AppEvent<NotificationMessage>? OnSuccessBroadcast { get; set; }

        public static AppEvent<NotificationMessage>? OnErrorBroadcast { get; set; }

        public static AppEvent<NotificationMessage>? OnWarningBroadcast { get; set; }

        public static void Info(string message, string? title = null)
        {
            OnInfoBroadcast?.Invoke(new NotificationMessage { Title = title, Message = message });
        }
        public static void Success(string message, string? title = null)
        {
            OnSuccessBroadcast?.Invoke(new NotificationMessage { Title = title, Message = message });
        }
        public static void Error(string message, string? title = null)
        {
            OnErrorBroadcast?.Invoke(new NotificationMessage { Title = title, Message = message });
        }
        public static void Warning(string message, string? title = null)
        {
            OnWarningBroadcast?.Invoke(new NotificationMessage { Title = title, Message = message });
        }
    }
}
