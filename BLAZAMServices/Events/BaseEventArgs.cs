using BLAZAM.Session.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public class BaseEventArgs:EventArgs
    {
        public Guid Guid { get; set; } = Guid.NewGuid();
        public IApplicationUserState Actor { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Username { get => Actor.AuditUsername; }
        public ApplicationEventType EventType { get; set; }
    }
}
