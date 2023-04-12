using System.Configuration;

namespace BLAZAM.Database.Models
{
    public class RecoverableAppDbSetBase : AppDbSetBase
    {
        /// <summary>
        /// The UTC <see cref="DateTime"/> this entry was deleted
        /// </summary>
        /// <remarks>
        /// Not deleted if <see cref="null"/>
        /// </remarks>
        public DateTime? DeletedAt { get; set; }


    }
}