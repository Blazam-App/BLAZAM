using BLAZAM.Database.Models.Notifications;
using BLAZAM.Session.Interfaces;

namespace BLAZAM.Services.Events
{

    public enum ApplicationEventType
    {
        All,
        Search,
        Create,
        Delete,
        Modify,
        Move,
        Unassign,
        Assign,
        PasswordChange,
        LockedOut,
        Scheduled
    }
    public static class ApplicationEventTypeHelpers
    {
        public static ApplicationEventType ToApplicationEventType(this NotificationType notificationType)
        {
            switch (notificationType)
            {
                case NotificationType.Create:
                    return ApplicationEventType.Create;
                case NotificationType.Delete:
                    return ApplicationEventType.Delete;
                case NotificationType.Modify:
                    return ApplicationEventType.Modify;
                case NotificationType.Assign:
                    return ApplicationEventType.Assign;
                case NotificationType.PasswordChange:
                    return ApplicationEventType.PasswordChange;
                case NotificationType.LockedOut:
                    return ApplicationEventType.LockedOut;
                case NotificationType.Scheduled:
                    return ApplicationEventType.Scheduled;
                default:
                    return ApplicationEventType.All;
            }
        }
    }


    public class BaseEventArgs : EventArgs
    {
        /// <summary>
        /// Used to check for duplicate events in scoped recievers
        /// </summary>
        public Guid Guid { get; set; } = Guid.NewGuid();
        /// <summary>
        /// The user state that performed the change
        /// </summary>
        public IApplicationUserState Actor { get; set; }
        /// <summary>
        /// The time the event was created
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        /// <summary>
        /// The username of the <see cref="Actor"/>
        /// </summary>
        public string Username { get => Actor.AuditUsername; }
        /// <summary>
        /// The type of this event
        /// </summary>
        public ApplicationEventType EventType { get; set; }
    }
}
