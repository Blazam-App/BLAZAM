using System.ComponentModel.DataAnnotations.Schema;

namespace BLAZAM.Common.Models.Database.Permissions
{
    public class PrivilegeLevel : IComparable
    {
        public int PrivilegeLevelId { get; set; }
        public byte[] GroupSID { get; set; }
        public bool IsSuperAdmin { get; set; }
        public List<PrivilegeMap> PrivilegeMaps { get; set; }

        public DateTime? DeletedAt { get; set; }

        public int CompareTo(object? obj)
        {
            if (obj != null && obj is PrivilegeLevel pl)
                return GroupSID.ToSidString().CompareTo(pl.GroupSID.ToSidString());
            return 0;
        }
        public override int GetHashCode()
        {
            return PrivilegeLevelId.ToString().GetHashCode();
        }
        public override bool Equals(object? obj)
        {
            if(obj is PrivilegeLevel l)
            {
                if(l.PrivilegeLevelId==this.PrivilegeLevelId || l.GroupSID == this.GroupSID)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
