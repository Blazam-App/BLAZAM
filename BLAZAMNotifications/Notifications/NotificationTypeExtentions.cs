using BLAZAM.Database.Models.Notifications;
using BLAZAM.EmailMessage.Email.Base; // Added
using BLAZAM.Logger; // Added

namespace BLAZAM.Notifications.Notifications
{
    /// <summary>
    /// Provides extension methods for the <see cref="BLAZAM.Database.Models.Notifications.NotificationType"/> enum.
    /// </summary>
    public static class NotificationTypeExtentions
    {
        /// <summary>
        /// Converts a <see cref="BLAZAM.Database.Models.Notifications.NotificationType"/> enum to its corresponding notification template component.
        /// </summary>
        /// <typeparam name="T">The expected base type of the notification component (should be <see cref="BLAZAM.EmailMessage.Email.Base.NotificationTemplateComponent"/> or a class derived from it).</typeparam>
        /// <param name="type">The notification type to convert.</param>
        /// <returns>An instance of the corresponding notification component cast to type T, or default(T) if no mapping exists for the given type or if an error occurs.</returns>
        public static T? ToNotification<T>(this NotificationType type) where T : NotificationTemplateComponent
        {
            NotificationTemplateComponent? notificationTemplate = null;
            switch (type)
            {
                case NotificationType.PasswordChange:
                    notificationTemplate = new PasswordChangedEmailMessage();
                    break;
                case NotificationType.Create:
                    notificationTemplate = new EntryCreatedEmailMessage();
                    break;
                case NotificationType.Delete:
                    notificationTemplate = new EntryDeletedEmailMessage();
                    break;
                case NotificationType.Modify:
                    notificationTemplate = new EntryEditedEmailMessage();
                    break;
                case NotificationType.Unassign:
                    notificationTemplate = new EntryUnassignedEmailMessage();
                    break;
                case NotificationType.Assign:
                    notificationTemplate = new EntryAssignedEmailMessage();
                    break;
                case NotificationType.LockedOut:
                    notificationTemplate = new LockedOutEmailMessage();
                    break;
                default: // Added default case
                    Loggers.SystemLogger.Warning("NotificationTypeExtentions.ToNotification: Unknown or unhandled NotificationType '{Type}' encountered. Returning default(T).", type);
                    return default; // Return default(T) directly from here
            }

            return (T?)notificationTemplate;

        }
    }
}
