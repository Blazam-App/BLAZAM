using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using System.ComponentModel.DataAnnotations.Schema;

namespace BLAZAM.Common.Models.Database
{
    public class AuthenticationSettings
    {
        public int? AuthenticationSettingsId { get; set; }
        public int? SessionTimeout { get; set; } = 15 * 60 * 1000;
        public string? AdminPassword { get; set; }
        public string? DuoClientId { get; set; }
        public string? DuoClientSecret { get; set; }
        public string? DuoApiHost { get; set; }
    }
}
