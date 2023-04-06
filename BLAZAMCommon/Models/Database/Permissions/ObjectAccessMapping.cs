using BLAZAM.Common.Data.ActiveDirectory;

namespace BLAZAM.Common.Models.Database.Permissions
{
    public class ObjectAccessMapping : RecoverableAppDbSetBase
    {
        public ActiveDirectoryObjectType ObjectType { get; set; }
        public int ObjectAccessLevelId { get; set; }
        public ObjectAccessLevel ObjectAccessLevel { get; set; }
        public List<AccessLevel> AccessLevels { get; set; }
        public bool AllowDisabled { get; set; }
    }
}