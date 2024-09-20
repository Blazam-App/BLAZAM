using BLAZAM.Database.Models.Notifications;

namespace BLAZAM.Notifications.Notifications
{
    public static class NotificationTypeExtentions
    {
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
                case NotificationType.GroupAssignment:
                    notificationTemplate = new EntryGroupAssignmentEmailMessage();
                    break;

            }
            if (notificationTemplate != null)
            {
                return (T?)notificationTemplate;
            }
            return default(T);
        }
    }
}
