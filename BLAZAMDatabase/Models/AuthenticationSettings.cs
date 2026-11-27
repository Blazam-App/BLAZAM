using BLAZAM.Common.Data.Validators;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BLAZAM.Database.Models
{
    public enum DuoUnreachableBehavior
    {
        Block,
        Bypass
    }
    public enum MfaType
    {
        CiscoDuo,
        GoogleAuthenticator
    }
    public class AuthenticationSettings : AppDbSetBase
    {
        /// <summary>
        /// Time a session can remain inactive before expiring in minutes
        /// </summary>
        public int? SessionTimeout { get; set; } = 15;
        [Required]
        [ValidAdminPasswordAttribute]
        public string? AdminPassword { get; set; }
        [NotMapped]
        [Required]
        [Compare(nameof(AdminPassword))]
        public string? AdminPasswordConfirmed { get; set; }
        public bool DuoEnabled { get; set; }
        /// <summary>
        /// Gets or sets the Duo client ID.
        /// </summary>
        public string? DuoClientId { get; set; }
        /// <summary>
        /// Gets or sets the Duo client secret ciphertext used for application authentication.
        /// </summary>
        /// <remarks>Stored as ciphertext that requires decryption for use.</remarks>
        public string? DuoClientSecret { get; set; }
        public string? DuoApiHost { get; set; }
        public DuoUnreachableBehavior DuoUnreachableBehavior { get; set; } = DuoUnreachableBehavior.Block;

        /// <summary>
        /// Indicates that MFA must be used by all users other than admimn and demo
        /// </summary>
        /// <remarks>
        /// If DUO is enabled this setting has no effect.
        /// </remarks>
        public bool RequireMFA { get; set; }
        public MfaType MFAType { get; set; }

        [NotMapped]
        public bool DuoSettingsValid
        {
            get
            {
                return DuoEnabled &&
                    DuoClientSecret != null &&
                    DuoClientId != null &&
                    DuoApiHost != null;
            }
        }
        /// <summary>
        /// Salt used when generating or validating API tokens.
        /// </summary>
        /// <remarks>Use a cryptographically strong, unique value and treat it as sensitive
        /// data.</remarks>
        public byte[]? ApiTokenSalt { get; set; } = null;
    }
}
