using System.ComponentModel.DataAnnotations.Schema;

namespace BLAZAM.Common.Models.Database.Audit
{
    [Table("SystemAuditLog", Schema = "Audit")]

    public class SystemAuditLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Username { get; set; } = "System";
        public string Action { get; set; }
    }
}
