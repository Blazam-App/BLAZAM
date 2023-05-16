using BLAZAM.Common.Data.Validators;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BLAZAM.Database.Models
{
    public class AuthenticationSettings : AppDbSetBase
    {
        /// <summary>
        /// Time a session can remain inactive before expiring in minutes
        /// </summary>
        public int? SessionTimeout { get; set; } = 15;
        [Required]
        [ValidAdminPassword]
        public string? AdminPassword { get; set; }
        [NotMapped]
        [Required]
        [Compare(nameof(AdminPassword))]
        public string AdminPasswordConfirmed { get; set; }

        public string? DuoClientId { get; set; }
        public string? DuoClientSecret { get; set; }
        public string? DuoApiHost { get; set; }
    }
}
