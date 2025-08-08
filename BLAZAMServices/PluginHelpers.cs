using BLAZAM.Helpers;
using BLAZAM.Plugins;
using Microsoft.AspNetCore.Components;
using System.Reflection;

namespace BLAZAM.Services
{
    internal static class PluginHelpers

    {
        internal static IEnumerable<Type> GetPluginComponents(this Assembly pluginAssembly)
        {
            return pluginAssembly.GetPluginTypes(typeof(ComponentBase));
        }

        internal static IEnumerable<PluginComponentAttribute> GetPluginComponentAttributes(this Type pluginComponent)
        {
            return pluginComponent.GetCustomAttributes(typeof(PluginComponentAttribute), false).Cast<PluginComponentAttribute>();
        }
    }

}
