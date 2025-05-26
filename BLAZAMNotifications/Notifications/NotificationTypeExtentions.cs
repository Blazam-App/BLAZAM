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
            // This part is only reached if a known type was handled and notificationTemplate was set.
            // If a default case returned, this is skipped.
            // The original logic of checking notificationTemplate != null before casting is fine,
            // but it's somewhat redundant now if the default case handles unknown types.
            // However, keeping it doesn't hurt and handles a hypothetical future case where a known type might still result in a null template.
            if (notificationTemplate != null)
            {
                return (T?)notificationTemplate;
            }
            // This return is now only reachable if a known type resulted in a null notificationTemplate,
            // or if the switch statement was somehow bypassed (which it shouldn't be).
            // The default case already handled unknown types.
            return default;
        }
    }
}
