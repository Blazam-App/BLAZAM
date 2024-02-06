
using BLAZAM.Helpers;
using System.ComponentModel.DataAnnotations.Schema;

namespace BLAZAM.Database.Models.Permissions
{
    public class PermissionDelegate : RecoverableAppDbSetBase, IComparable
    {
        public byte[] DelegateSid { get; set; }
        public bool IsSuperAdmin { get; set; }
        public List<PermissionMapping> PermissionsMaps { get; set; }


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
            if (obj is PermissionDelegate l)
            {
                if (l.Id == Id || l.DelegateSid == DelegateSid)
                {
                    return true;
                }
            }
            return false;
        }

        public override string? ToString()
        {
            if(!DelegateName.IsNullOrEmpty())
            {
                return DelegateName;
            }
            return base.ToString();
        }
    }
}
