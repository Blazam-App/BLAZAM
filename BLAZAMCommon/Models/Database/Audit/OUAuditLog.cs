using System.ComponentModel.DataAnnotations.Schema;

namespace BLAZAM.Common.Models.Database.Audit
{
    [Table("OUAuditLog", Schema = "Audit")]

    public class OUAuditLog : CommonAuditLog
    {
    }
}
