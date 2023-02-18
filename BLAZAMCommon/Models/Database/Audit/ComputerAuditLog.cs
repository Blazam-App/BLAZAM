using System.ComponentModel.DataAnnotations.Schema;

namespace BLAZAM.Common.Models.Database.Audit
{
    [Table("ComputerAuditLog", Schema = "Audit")]
    public class ComputerAuditLog  : CommonAuditLog
    {
    }
}
