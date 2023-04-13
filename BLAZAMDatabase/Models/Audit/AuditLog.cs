using System.Net;

namespace BLAZAM.Database.Models.Audit
{
    public class CommonAuditLog : AppDbSetBase, ICommonAuditLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Username { get; set; }
        public string? IpAddress { get; set; }
        public string Action { get; set; }
        public string? Target { get; set; }
        public string? BeforeAction { get; set; }
        public string? AfterAction { get; set; }

    }
}
