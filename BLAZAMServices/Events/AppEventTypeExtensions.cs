using BLAZAM.Database.Models.Notifications;

namespace BLAZAM.Services.Events
{
    public static class AppEventTypeExtensions
    {
        public static NotificationType ToNotificationType(this ApplicationEventType eventType)
        {
            switch (eventType)
            {
                case ApplicationEventType.Assign:
                    return NotificationType.Assign;
                case ApplicationEventType.Move:
                case ApplicationEventType.Modify:
                    return NotificationType.Modify;
                case ApplicationEventType.PasswordChange:
                    return NotificationType.PasswordChange;
                case ApplicationEventType.Create:
                    return NotificationType.Create;
                case ApplicationEventType.Delete:
                    return NotificationType.Delete;
                case ApplicationEventType.LockedOut:
                    return NotificationType.LockedOut;
                case ApplicationEventType.Scheduled:
                    return NotificationType.Scheduled;
                case ApplicationEventType.Unassign:
                    return NotificationType.Unassign;
                default:
                    return NotificationType.Create;
            }

        }
    }
}
