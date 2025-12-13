namespace BLAZAM.Database.Models.Permissions
{
    public class GlobalPermissionSettings : AppDbSetBase
    {
        public bool AllowSelfModification { get; set; }

        public bool AllowActionAccessRequest { get; set; }

    }
}
