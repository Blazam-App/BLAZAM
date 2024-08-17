using System.Security.Claims;

namespace BLAZAM.Common.Data
{
    public static class UserRoles
    {
        /// <summary>
        /// All roles, except <see cref="SuperAdmin"/>
        /// </summary>
        public static List<string> All = new List<string>(){
            Login,
            SearchComputers,
            SearchGroups,
            SearchOUs,
            SearchUsers,
            CreateGroups,
            CreateOUs,
            CreateUsers,
            SearchPrinters,
            CreatePrinters,
            SearchBitLocker
        };
        public const string Login = "Login";
        public const string SearchUsers = "SearchUsers";
        public const string CreateUsers = "CreateUsers";
        public const string SearchGroups = "SearchGroups";
        public const string CreateGroups = "CreateGroups";
        public const string SearchOUs = "SearchOUs";
        public const string CreateOUs = "CreateOUs";
        public const string SearchPrinters = "SearchPrinters";
        public const string CreatePrinters = "CreatePrinters";
        public const string SearchComputers = "SearchComputers";
        public const string SearchBitLocker = "SearchBitLocker";

        public const string SuperAdmin = "SuperAdmin";
    }
}
