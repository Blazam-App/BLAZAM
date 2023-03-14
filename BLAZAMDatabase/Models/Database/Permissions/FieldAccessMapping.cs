
namespace BLAZAM.Database.Models.Database.Permissions
{
    public class FieldAccessMapping
    {
        public int FieldAccessMappingId { get; set; }
        public int ActiveDirectoryFieldId { get; set; }
        public ActiveDirectoryField Field { get; set; }
        public int FieldAccessLevelId { get; set; }
        public FieldAccessLevel FieldAccessLevel { get; set; }
        public List<AccessLevel> AccessLevels { get; set; }

    }
}
