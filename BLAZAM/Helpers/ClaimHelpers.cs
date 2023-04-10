using BLAZAM.Common.Data;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Services.UserAccountMapping;
using System.Collections;
using System.Security.Claims;

namespace BLAZAM.Server.Helpers
{
    public static class ClaimHelpers
    {
        public static void AddSuperAdmin(this IList<Claim> claims)
        {
            claims.Add(new Claim(ClaimTypes.Role, UserRoles.SuperAdmin));
        }



        /// <summary>
        /// Adds all roles except <see cref="UserRoles.SuperAdmin"/>
        /// </summary>
        /// <param name="claims"></param>
        public static void AddAllRoles(this IList<Claim> claims)
        {
            foreach (var role in UserRoles.All)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

    }
}
