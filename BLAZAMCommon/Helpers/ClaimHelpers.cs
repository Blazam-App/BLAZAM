using BLAZAM.Common.Data;
using System.Security.Claims;

namespace BLAZAM.Server.Helpers
{
    /// <summary>
    /// Provides extension methods for conveniently adding specific roles as claims to an <see cref="IList{Claim}"/>.
    /// </summary>
    public static class ClaimHelpers
    {
        /// <summary>
        /// Adds the SuperAdmin role claim (<see cref="UserRoles.SuperAdmin"/>) to the provided claims list.
        /// </summary>
        /// <param name="claims">The list of claims to add to.</param>
        /// <exception cref="ArgumentNullException">Thrown if the claims collection is null.</exception>
        public static void AddSuperAdmin(this IList<Claim> claims)
        {

            ArgumentNullException.ThrowIfNull(claims);

            
            claims.Add(new Claim(ClaimTypes.Role, UserRoles.SuperAdmin));
        }

        /// <summary>
        /// Adds all application role claims defined in <see cref="UserRoles.All"/> to the provided claims list.
        /// </summary>
        /// <param name="claims">The list of claims to add to.</param>
        /// <exception cref="ArgumentNullException">Thrown if the claims collection is null.</exception>
        public static void AddAllRoles(this IList<Claim> claims)
        {

            ArgumentNullException.ThrowIfNull(claims);

            foreach (var role in UserRoles.All)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }
    }
}
