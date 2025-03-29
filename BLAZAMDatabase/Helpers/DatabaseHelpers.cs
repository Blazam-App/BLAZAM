using BLAZAM.Common.Data;
using BLAZAM.Database.Models.Permissions;
using BLAZAM.Database.Models.User;

namespace BLAZAM.Helpers
{
    public static class DatabaseHelpers
    {
        public static long GetMembersHash(this IEnumerable<AppUser> members)
        {
            long hash = 0;
            foreach (var member in members)
            {
                hash += member.Username.GetAppHashCode();
            }
            return hash;
        }
        public static List<ActiveDirectoryObjectAction> ToList(this ActiveDirectoryObjectAction _enum)
        {
            return Enum.GetValues(typeof(ActiveDirectoryObjectAction)).Cast<ActiveDirectoryObjectAction>().ToList();
        }
        public static bool IsAdminOrDemo(this AppUser state)
        {
            return state.Username?.Equals("admin", StringComparison.InvariantCultureIgnoreCase) == true || state.Username?.Equals("demo", StringComparison.InvariantCultureIgnoreCase) == true;
        }
       
    }
}
