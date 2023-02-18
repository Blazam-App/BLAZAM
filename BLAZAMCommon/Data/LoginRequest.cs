using System.ComponentModel.DataAnnotations;
using System.Security;
using System.Security.Claims;

namespace BLAZAM.Common.Data
{
    public class LoginRequest
    {
        private SecureString? password;

        [Required]
        public string? Username { get; set; }
        [Required]
        public string? Password { get => password.ToPlainText(); set => password = value.ToSecureString(); }
        public SecureString? SecurePassword => password;
        public string? ReturnUrl { get; set; } = "/";
        public bool Valid
        {
            get
            {
                if (Username != null && Username.Length > 0)
                    if (Password != null && Password.Length > 0)
                        return true;
                return false;
            }
        }

        public bool Impersonation { get; set; } = false;
        public ClaimsPrincipal? ImpersonatorClaims { get; set; }
    }
}
