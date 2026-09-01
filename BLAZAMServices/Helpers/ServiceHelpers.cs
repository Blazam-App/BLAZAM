using BLAZAM.ActiveDirectory.Data;
using BLAZAM.Session.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLAZAM.Services.Helpers
{
    public static class ServiceHelpers
    {
        public static ActiveDirectoryUserState ToActiveDirectoryUserState(this IApplicationUserState userState)
        {
            return new ActiveDirectoryUserState()
            {
                Username = userState.AuditUsername,
                PermissionMappings = userState.PermissionMappings,
                IsSuperAdmin = userState.IsSuperAdmin

            };

        }
    }
}
