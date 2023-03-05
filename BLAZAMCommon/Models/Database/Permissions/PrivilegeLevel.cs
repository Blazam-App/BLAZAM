using System.ComponentModel.DataAnnotations.Schema;

namespace BLAZAM.Common.Models.Database.Permissions
{
    public class PermissionDelegate : IComparable
    {
        public int Id { get; set; }
        public byte[] DelegateSid { get; set; }
        public bool IsSuperAdmin { get; set; }
        public List<PermissionMap> PermissionsMaps { get; set; }

        public DateTime? DeletedAt { get; set; }

        [NotMapped]
        public string? DelegateName { get; set; }
        
        public int CompareTo(object? obj)
        {
            if (obj != null && obj is PermissionDelegate pl)
                return DelegateSid.ToSidString().CompareTo(pl.DelegateSid.ToSidString());
            return 0;
        }
        public override int GetHashCode()
        {
            return Id.ToString().GetHashCode();
        }
        public override bool Equals(object? obj)
        {
            if(obj is PermissionDelegate l)
            {
                if(l.Id==this.Id || l.DelegateSid == this.DelegateSid)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
