using System.ComponentModel.DataAnnotations.Schema;

namespace BLAZAM.Common.Models.Database.Audit
{
    [Table("UserAuditLog", Schema = "Audit")]

    public class UserAuditLog : CommonAuditLog
    {
    }
}
