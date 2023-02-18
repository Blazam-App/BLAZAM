using System.ComponentModel.DataAnnotations.Schema;

namespace BLAZAM.Common.Models.Database.Audit
{
    [Table("GroupAuditLog", Schema = "Audit")]

    public class GroupAuditLog : CommonAuditLog
    {
    }
}
