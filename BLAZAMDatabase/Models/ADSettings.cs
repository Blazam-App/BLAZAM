
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BLAZAM.Common.Data.Validators;
using BLAZAM.Helpers;

namespace BLAZAM.Database.Models
{
    public class ADSettings : AppDbSetBase
    {
        [Required(ErrorMessage = "The base DN is required.")]
        public string ApplicationBaseDN { get; set; }
        [ValidFqdnAttribute]
        public string FQDN { get; set; }
        [Required(ErrorMessage = "The server address is required.")]
        public string ServerAddress { get; set; }
        [Required(ErrorMessage = "The server port is required.")]
        public int ServerPort { get; set; } = 389;
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        public bool UseTLS { get; set; } = false;

        [NotMapped]
        public bool IsValid
        {
            get
            {
                return (!FQDN.IsNullOrEmpty()
                    && !Username.IsNullOrEmpty()
                    && !Password.IsNullOrEmpty()
                    && !ServerAddress.IsNullOrEmpty()
                    && ServerPort != 0);
            }
        }
    }
}
