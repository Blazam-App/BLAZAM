using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Database.Models.User
{
    public class ApiToken : RecoverableAppDbSetBase
    {
        /// <summary>
        /// The <see cref="JwtSecurityToken"/> string in encrypted form.
        /// </summary>
        public string Token { get; set; }

        public int TokenHash { get; set; }

        /// <summary>
        /// The UTC <see cref="DateTime"/> the token was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The UTC <see cref="DateTime"/> the token will expire at.
        /// </summary>
        public DateTime ExpiresAt { get; set; }


        /// <summary>
        /// The UTC <see cref="DateTime"/> the token was last used.
        /// </summary>
        /// <remarks>
        /// Null if never used.
        /// </remarks>
        public DateTime? LastUsedAt { get; set; }

        /// <summary>
        /// The <see cref="AppUser"/> the token belongs to.
        /// </summary>
        public AppUser User { get; set; }

        /// <summary>
        /// The <see cref="Id"/> of the <see cref="AppUser"/>.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// A description of the token's purpose.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Indicates whether the token has been revoked.
        /// </summary>
        public bool IsRevoked { get; set; }
    }
}
