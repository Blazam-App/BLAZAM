using BLAZAM.Common.Data.Validators;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BLAZAM.Common.Models.Database
{
    public class AuthenticationSettings : AppDbSetBase
    {

        public int? SessionTimeout { get; set; } = 900;
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
