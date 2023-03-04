
using System.ComponentModel.DataAnnotations;

namespace BLAZAM.Common.Models.Database.Permissions
{
    public class AccessLevel:IComparable<AccessLevel>
    {
        public int AccessLevelId { get; set; }
        [Required]
        public string Name { get; set; }
        public List<ObjectAccessMapping> ObjectMap { get; set; } = new();
        public List<ActionAccessMapping> ActionMap { get; set; } = new();
        public List<FieldAccessMapping> FieldMap { get; set; } = new();
        public List<PermissionMap> PermissionMaps { get; set; }
        public int CompareTo(AccessLevel? other)
        {
            if(other == null) return 1;
            return this.AccessLevelId.CompareTo(other.AccessLevelId);
        }

        public override bool Equals(object? obj)
        {
            if(obj is AccessLevel al)
            {
                if(al.AccessLevelId==AccessLevelId) return true;
            }
            return base.Equals(obj);
        }
    }
}
