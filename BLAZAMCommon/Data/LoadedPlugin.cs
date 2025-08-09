using System.Reflection;
using BLAZAM.Plugins;

namespace BLAZAM.Common.Data
{
    public class LoadedPlugin
    {
        public LoadedPlugin(Assembly assembly, IPluginBase pluginInstance)
        {
            Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly), "Assembly cannot be null.");
            PluginBase = pluginInstance ?? throw new ArgumentNullException(nameof(pluginInstance), "Plugin instance cannot be null.");
        }

        public Assembly Assembly { get; }
        public IPluginBase PluginBase { get; }

    }
}