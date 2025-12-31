using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Database.Models.User
{
    public class UserPasswordResetSettings: AppDbSetBase
    {
        public AppUser User{ get; set; }
        public int UserId{ get; set; }
        /// <summary>
        /// Encrypted PIN for password reset
        /// </summary>
        public string? PIN { get; set; }
        /// <summary>
        /// Encrypted security questions and answers for password reset
        /// </summary>
        public string? Question1{ get; set; }
        /// <summary>
        /// Encrypted security questions and answers for password reset
        /// </summary>
        public string? Answer1{ get; set; }
        /// <summary>
        /// Encrypted security questions and answers for password reset
        /// </summary>
        public string? Question2{ get; set; }
        /// <summary>
        /// Encrypted security questions and answers for password reset
        /// </summary>
        public string? Answer2 { get; set; }
        /// <summary>
        /// Encrypted security questions and answers for password reset
        /// </summary>

        public string? Question3{ get; set; }
        /// <summary>
        /// Encrypted security questions and answers for password reset
        /// </summary>
        public string? Answer3 { get; set; }
        /// <summary>
        /// Encrypted reset token for password reset
        /// </summary>
        public string? ResetToken { get; set; }
        /// <summary>
        /// Gets or sets the encrypted expiration date and time of the token.
        /// </summary>
        public string? TokenExpiration { get; set; }
    }
}
