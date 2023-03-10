using BLAZAM.Common.Data.ActiveDirectory;
using System.ComponentModel.DataAnnotations.Schema;

namespace BLAZAM.Common.Models.Database.Permissions
{
    public class ObjectAccessMapping
    {
        public int ObjectAccessMappingId { get; set; }
        public ActiveDirectoryObjectType ObjectType { get; set; }
        public int ObjectAccessLevelId { get; set; }
        public ObjectAccessLevel ObjectAccessLevel { get; set; }
        public List<AccessLevel> AccessLevels { get; set; }
        public bool AllowDisabled { get; set; }
    }
}