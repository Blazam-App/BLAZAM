using BLAZAM.ActiveDirectory.Data;
using BLAZAM.Session.Interfaces;

namespace BLAZAM.Helpers
{
    public static class ActiveDirectoryHelpers
    {
        public static ActiveDirectoryUserState ToActiveDirectoryUserState(this IApplicationUserState userState)
        {
            return new ActiveDirectoryUserState()
            {
                Username = userState.AuditUsername,
                AuditActor = userState.AuditActor,
                PermissionMappings = userState.PermissionMappings,
                IsSuperAdmin = userState.IsSuperAdmin

            };

        }
    }
}
