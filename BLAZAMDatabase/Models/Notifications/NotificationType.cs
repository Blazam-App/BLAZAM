using System.Runtime.CompilerServices;

namespace BLAZAM.Database.Models.Notifications
{
    public enum NotificationType
    {
        Create,
        Delete,
        Modify,
        Unassign,
        Assign,
        PasswordChange,
        LockedOut,
        Scheduled
    }

}
