
using BLAZAM.Helpers;
using System.ComponentModel;

namespace BLAZAM.Database.Models.Permissions
{
    public class PermissionDelegate : RecoverableAppDbSetBase, IComparable, IEquatable<PermissionDelegate?>
    {
        public byte[] DelegateSid { get; set; }
        public bool IsSuperAdmin { get; set; }
        public List<PermissionMapping> PermissionsMaps { get; set; }
        public string? DelegateName { get; set; }

        public bool AllowPasswordReset { get; set; } 
        public bool RequireEmailOnPasswordReset { get; set; } = true;
        public bool RequirePINOnPasswordReset { get; set; }

        public int MinimumPINLength { get; set; } = 4;
        public bool RequireQAOnPasswordReset { get; set; }

        public int CompareTo(object? obj)
        {
            if (obj is PermissionDelegate pl)
            {
                return DelegateSid.ToSidString().CompareTo(pl.DelegateSid.ToSidString());
            }

            return 0;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as PermissionDelegate);
        }

        public bool Equals(PermissionDelegate? other)
        {
            return other is not null &&
                   base.Equals(other) &&
                   Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id.ToString().GetHashCode();
        }


        public override string? ToString()
        {
            if (!DelegateName.IsNullOrEmpty())
            {
                return DelegateName;
            }
            return base.ToString();
        }

        public static bool operator ==(PermissionDelegate? left, PermissionDelegate? right)
        {
            return EqualityComparer<PermissionDelegate>.Default.Equals(left, right);
        }

        public static bool operator !=(PermissionDelegate? left, PermissionDelegate? right)
        {
            return !(left == right);
        }
    }
}
