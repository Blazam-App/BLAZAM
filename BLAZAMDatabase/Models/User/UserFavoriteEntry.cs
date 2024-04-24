

using System.ComponentModel.DataAnnotations.Schema;

namespace BLAZAM.Database.Models.User
{
   
    public class UserFavoriteEntry : AppDbSetBase
    {
        public string DN { get; set; }

        public AppUser User { get; set; }
        public int UserId { get; set; }

        public override bool Equals(object? obj)
        {
            if(obj is UserFavoriteEntry user)
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
    }
}
