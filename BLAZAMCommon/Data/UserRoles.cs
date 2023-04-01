using System.Security.Claims;

namespace BLAZAM.Common.Data
{
    public static class UserRoles
    {
       public const string Login = "Login";
        public const string SearchUsers = "SearchUsers";
        public const string CreateUsers = "CreateUsers";
        public const string SearchGroups = "SearchGroups";
        public const string CreateGroups = "CreateGroups";
        public const string SearchOUs = "SearchOUs";
        public const string SearchComputers = "SearchComputers";
        public const string CreateOUs = "CreateOUs";
        public const string SuperAdmin = "SuperAdmin";
    }
}
