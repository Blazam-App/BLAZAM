using BLAZAM.Common.Data;


namespace BLAZAM.Database.Models.Permissions
{
    public class FieldAccessMapping : AppDbSetBase
    {
        public ActiveDirectoryObjectType ObjectType { get; set; }

        // public int ActiveDirectoryFieldId { get; set; }
        public ActiveDirectoryField? Field { get; set; }
        public CustomActiveDirectoryField? CustomField { get; set; }
        public int FieldAccessLevelId { get; set; }
        public FieldAccessLevel FieldAccessLevel { get; set; }
        public List<AccessLevel> AccessLevels { get; set; }

    }
}
