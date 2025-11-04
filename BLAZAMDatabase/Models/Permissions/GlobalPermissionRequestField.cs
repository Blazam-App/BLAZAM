using Microsoft.Identity.Client;

namespace BLAZAM.Database.Models.Permissions
{
    public class GlobalPermissionRequestField : AppDbSetBase
    {
        public bool AllowEdit { get; set; }
        public ActiveDirectoryField? Field { get; set; }
        public CustomActiveDirectoryField? CustomField { get; set; }

    }
}
