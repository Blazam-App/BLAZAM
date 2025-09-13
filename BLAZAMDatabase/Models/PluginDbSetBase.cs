using System;
using System.ComponentModel.DataAnnotations;

namespace BLAZAM.Database.Models
{
    /// <summary>
    /// A base class for all plugin database models.
    /// </summary>
    public abstract class PluginDbSetBase : AppDbSetBase
    {
        /// <summary>
        /// The unique identifier for the plugin that owns this data.
        /// </summary>
        [Required]
        public Guid PluginId { get; set; }
    }
}
