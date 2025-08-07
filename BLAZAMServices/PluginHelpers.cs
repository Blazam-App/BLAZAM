using BLAZAM.Helpers;
using BLAZAM.Plugins;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
