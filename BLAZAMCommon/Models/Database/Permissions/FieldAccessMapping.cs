using BLAZAM.Common.Data.ActiveDirectory;

namespace BLAZAM.Common.Models.Database.Permissions
{
    public class FieldAccessMapping : AppDbSetBase
    {
        public int ActiveDirectoryFieldId { get; set; }
        public ActiveDirectoryField Field { get; set; }
        public int FieldAccessLevelId { get; set; }
        public FieldAccessLevel FieldAccessLevel { get; set; }
        public List<AccessLevel> AccessLevels { get; set; }

    }
}
