using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using BLAZAM.Common.Data.Validators;

namespace BLAZAM.Database.Models
{
    public class EmailSettings : AppDbSetBase
    {
        public bool Enabled { get; set; } = false;
        [DisplayName("Admin BCC")]
        public string? AdminBcc { get; set; }
        [DisplayName("From Display Name")]
        public string? FromName { get; set; }
        [DisplayName("Reply-To Address")]
        [EmailAddress(ErrorMessage = "Invalid email.")]
        public string? ReplyToAddress { get; set; }
        public string? ReplyToName { get; set; }
        [DisplayName("From Address")]
        [EmailAddress(ErrorMessage = "Invalid email.")]
        public string? FromAddress { get; set; }
        public bool UseSMTPAuth { get; set; } = false;
        public string? SMTPUsername { get; set; }
        public string? SMTPPassword { get; set; }
        [Required]
        [ValidIpOrFqdn]
        public string SMTPServer { get; set; }
        [ValidPortAttribute]
        public int SMTPPort { get; set; } = 25;
        public bool UseTLS { get; set; } = false;

        public bool Valid()
        {
            if (SMTPServer != null)
            {
                if (!UseSMTPAuth && FromAddress != null || UseSMTPAuth && SMTPUsername != null && SMTPPassword != null)
                {
                    return true;
                }
            }
            return false;
        }


    }
}
