using System.ComponentModel.DataAnnotations.Schema;

namespace BLAZAM.Common.Models.Database.Audit
{
    [Table("LogonAuditLog", Schema = "Audit")]

    public class LogonAuditLog : CommonAuditLog
    {
    }
}
