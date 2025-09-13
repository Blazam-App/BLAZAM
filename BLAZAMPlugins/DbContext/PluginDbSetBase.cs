using System.ComponentModel.DataAnnotations;
using BLAZAM.Global.Data;

namespace BLAZAM.Plugins.DbContext
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
