using System.ComponentModel.DataAnnotations.Schema;

namespace BLAZAM.Common.Models.Database.Audit
{
    [Table("PermissionsAuditLog", Schema = "Audit")]

    public class PermissionsAuditLog : CommonAuditLog
    {
    }
}
