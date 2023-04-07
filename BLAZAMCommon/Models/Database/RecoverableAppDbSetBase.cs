using System.Configuration;

namespace BLAZAM.Common.Models.Database
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