using BLAZAM.Global.Data;
using BLAZAM.Global.Enums;
using BLAZAM.Helpers;
using System.Net;

namespace BLAZAM.Global.Events
{
    public class BaseEventArgs : EventArgs
    {
        /// <summary>
        /// Used to check for duplicate events in scoped recievers
        /// </summary>
        public Guid Guid { get; set; } = Guid.NewGuid();
        /// <summary>
        /// The user state that performed the change
        /// </summary>
        public AuditLogActor Actor { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        /// <summary>
        /// The type of this event
        /// </summary>
        public ApplicationEventType EventType { get; set; }
    }
}
