using Microsoft.Identity.Client;

namespace BLAZAM.Database.Models.Permissions
{
    public class GlobalPermissionRequestField : ActiveDirectoryFieldDbSet
    {
        public bool AllowEdit { get; set; }

    }
}
