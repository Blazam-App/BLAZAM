using System.ComponentModel.DataAnnotations.Schema;

namespace BLAZAM.Common.Models.Database.Audit
{
    [Table("RequestAuditLog", Schema = "Audit")]

    public class RequestAuditLog : CommonAuditLog
    {
    }
}
