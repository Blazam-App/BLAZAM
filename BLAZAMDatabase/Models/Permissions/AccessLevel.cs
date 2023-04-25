
using System.ComponentModel.DataAnnotations;

namespace BLAZAM.Database.Models.Permissions
{
    public class AccessLevel : RecoverableAppDbSetBase, IComparable<AccessLevel>
    {
        [Required]
        public string Name { get; set; }
        public List<ObjectAccessMapping> ObjectMap { get; set; } = new();
        public List<ActionAccessMapping> ActionMap { get; set; } = new();
        public List<FieldAccessMapping> FieldMap { get; set; } = new();
        public List<PermissionMapping> PermissionMaps { get; set; }



        public int CompareTo(AccessLevel? other)
        {
            if (other == null) return 1;
            return Id.CompareTo(other.Id);
        }

        public override int GetHashCode()
        {
            return Id.ToString().GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            if (obj is AccessLevel al)
            {
                if (al.Id == Id) return true;
            }
            return base.Equals(obj);
        }
    }
}
