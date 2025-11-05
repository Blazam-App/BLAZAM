using BLAZAM.Common.Data;


namespace BLAZAM.Database.Models.Permissions
{
    public class FieldAccessMapping : ActiveDirectoryFieldDbSet
    {
        public ActiveDirectoryObjectType ObjectType { get; set; }

        public int FieldAccessLevelId { get; set; }
        public FieldAccessLevel FieldAccessLevel { get; set; }
        public List<AccessLevel> AccessLevels { get; set; }

    }
}
