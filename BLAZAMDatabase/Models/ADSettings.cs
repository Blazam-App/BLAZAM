
using BLAZAM.Common.Data.Validators;
using BLAZAM.Helpers;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BLAZAM.Database.Models
{
    public class ADSettings : AppDbSetBase
    {
        /// <summary>
        /// Gets or sets the application's base distinguished name (DN).
        /// </summary>
        /// <remarks>Required.</remarks>
        [Required(ErrorMessage = "The base DN is required.")]
        public string ApplicationBaseDN { get; set; }
        /// <summary>
        /// Gets or sets the fully qualified domain name (FQDN).
        /// </summary>
        [ValidFqdnAttribute]
        public string FQDN { get; set; }
        /// <summary>
        /// Gets or sets the server address.
        /// </summary>
        /// <remarks>Required.</remarks>
        [Required(ErrorMessage = "The server address is required.")]
        public string ServerAddress { get; set; }
        /// <summary>
        /// Gets or sets the server port number.
        /// </summary>
        /// <remarks>Required. The default value is 389.</remarks>
        [Required(ErrorMessage = "The server port is required.")]
        public int ServerPort { get; set; } = 389;
        /// <summary>
        /// Gets or sets the user name used to identify the account.
        /// </summary>
        /// <remarks>This value is required.</remarks>
        [Required]
        public string Username { get; set; }
        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <remarks>Marked as required for validation.</remarks>
        [Required]
        public string Password { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether to use TLS (Transport Layer Security) for the LDAP connection.
        /// </summary>
        public bool UseTLS { get; set; } = false;

        /// <summary>
        /// Gets or sets the maximum number of search results to return.
        /// </summary>
        /// <remarks>The default value is 3000.</remarks>
        public int MaxSearchResults { get; set; } = 3000;

        /// <summary>
        /// Gets a value indicating whether all required connection settings are present.
        /// </summary>
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
