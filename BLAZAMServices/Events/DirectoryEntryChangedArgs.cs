using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Models.Notifications;
using BLAZAM.Session.Interfaces;
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
    public class DirectoryEntryChangedArgs:BaseEventArgs
    {
        public IDirectoryEntryAdapter Entry { get; set; }
        public List<AuditChangeLog> Changes { get; set; }

        public IDirectoryEntryAdapter? Target { get; set; }
        public IDirectoryEntryAdapter? Origin { get; set; }
        public IDirectoryEntryAdapter? OriginalEntry { get; set; }
        public ActiveDirectoryObjectType ObjectType { get=>Entry.ObjectType;}
        public AppEventType EventType { get; set; }

    }
}
