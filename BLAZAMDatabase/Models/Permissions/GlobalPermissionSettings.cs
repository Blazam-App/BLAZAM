namespace BLAZAM.Database.Models.Permissions
{
    public class GlobalPermissionSettings : AppDbSetBase
    {
        public bool AllowSelfModification { get; set; }

        public bool AllowActionAccessRequest { get; set; }
        public bool AllowFieldAccessRequest { get; set; }

        public List<GlobalPermissionRequestField> RequestableFields { get; set; } = new();

    }
}
