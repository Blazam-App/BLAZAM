using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Models.Notifications;
using BLAZAM.Session.Interfaces;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Services.Events
{
    public enum AppEventType
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
    public static class AppEventTypeExtensions
    {
        public static NotificationType ToNotificationType(this AppEventType eventType)
        {
            switch (eventType)
            {
                case AppEventType.Assign:
                    return NotificationType.Assign;
                case AppEventType.Move:
                case AppEventType.Modify:
                    return NotificationType.Modify;
                case AppEventType.PasswordChange:
                    return NotificationType.PasswordChange;
                case AppEventType.Create:
                    return NotificationType.Create;
                case AppEventType.Delete:
                    return NotificationType.Delete;
                case AppEventType.LockedOut:
                    return NotificationType.LockedOut;
                case AppEventType.Scheduled:
                    return NotificationType.Scheduled;
                case AppEventType.Unassign:
                    return NotificationType.Unassign;
                default:
                    return NotificationType.Create;
            }

        }
    }
    public class DirectoryEntryChangedArgs : BaseEventArgs
    {
        public IDirectoryEntryAdapter Entry { get; set; }
        public List<AuditChangeLog> Changes { get; set; }

        public IDirectoryEntryAdapter? Target { get; set; }
        public IDirectoryEntryAdapter? Origin { get; set; }
        public IDirectoryEntryAdapter? OriginalEntry { get; set; }
        public ActiveDirectoryObjectType ObjectType { get => Entry.ObjectType; }
        public AppEventType EventType { get; set; }

    }
}
