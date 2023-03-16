using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;

namespace BLAZAM.Common.Models.Database
{
    public class AuthenticationSettings
    {
        public int? AuthenticationSettingsId { get; set; }
        public int? SessionTimeout { get; set; }
        public string? AdminPassword { get; set; }
        public string? DuoClientId { get; set; }
        public string? DuoClientSecret { get; set; }
        public string? DuoApiHost { get; set; }
    }
}
