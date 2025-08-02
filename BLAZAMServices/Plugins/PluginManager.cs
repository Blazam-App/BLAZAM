using BLAZAM.Helpers;
using BLAZAM.Plugins;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Services.Plugins
{

    /// <summary>
    /// Manages the loading, registration, and lifecycle of external plugins.
    /// This service is intended to provide extensibility to the application. (Functionality TBD)
    /// </summary>
    public static class PluginManager
    {

        public static Type? GetPluginSettingsComponent(IPluginBase plugin)
        {
            var matchingPlugin = ApplicationInfo.LoadedPlugins.FirstOrDefault(p => p.Value.Equals(plugin));
            if (matchingPlugin.Key != null)
            {
                var settingsPage = matchingPlugin.Key.GetPluginTypes(typeof(ComponentBase));
                foreach (Type pluginRenderFragment in settingsPage)
                {
                    var attribute = pluginRenderFragment.GetCustomAttributes(typeof(PluginRenderFragmentAttribute), false).FirstOrDefault() as PluginRenderFragmentAttribute;
                    if (attribute != null)
                    {
                        if (attribute.PageType == PageType.Plugin && attribute.PageLocation == PageLocation.Settings)
                        {
                            return pluginRenderFragment;

                        }
                    }
                }


            }
            return null;
        }
    }
}
