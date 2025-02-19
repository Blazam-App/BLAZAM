namespace BLAZAM.Database.Models.User
{

    public class UserFavoriteEntry : AppDbSetBase, IEquatable<UserFavoriteEntry?>
    {
        public string DN { get; set; }

        public AppUser User { get; set; }
        public int UserId { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is UserFavoriteEntry user)
            {
                if (user.DN == null) return false;
                return user.DN.Equals(DN) && user.UserId.Equals(UserId);
            }
            return false;
        }

        public override bool Equals(AppDbSetBase? other)
        {
            return Equals((object)other);
        }

        public bool Equals(UserFavoriteEntry? other)
        {
            return other is not null &&
                   DN == other.DN &&
                   UserId == other.UserId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), DN, UserId);
        }

        public static bool operator ==(UserFavoriteEntry? left, UserFavoriteEntry? right)
        {
            return EqualityComparer<UserFavoriteEntry>.Default.Equals(left, right);
        }

        public static bool operator !=(UserFavoriteEntry? left, UserFavoriteEntry? right)
        {
            return !(left == right);
        }
    }
}
