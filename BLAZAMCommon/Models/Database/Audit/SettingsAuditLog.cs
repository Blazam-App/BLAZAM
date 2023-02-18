using System.ComponentModel.DataAnnotations.Schema;

namespace BLAZAM.Common.Models.Database.Audit
{
    [Table("SettingsAuditLog", Schema = "Audit")]

    public class SettingsAuditLog : CommonAuditLog
    {
    }
}
