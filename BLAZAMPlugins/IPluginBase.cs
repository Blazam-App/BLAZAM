using BLAZAM.Global.Data;

namespace BLAZAM.Plugins
{
    /// <summary>
    /// Defines the base interface for plugins, providing essential metadata and functionality.
    /// </summary>
    /// <remarks>Implementations of this interface represent plugins that can be dynamically loaded and
    /// utilized within an application. The interface exposes metadata such as the plugin's name, version, and author,
    /// as well as the associated assembly.</remarks>
    public interface IPluginBase
    {
        /// <summary>
        /// The name of the plugin.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// The version of the plugin.
        /// </summary>
        PluginVersion Version { get; }
        /// <summary>
        /// The author of the plugin.
        /// </summary>
        string Author { get; }



       
    }
}
