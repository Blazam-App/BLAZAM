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
        public static List<Type> GetPluginTypeForPageAndLocation(PageType pageType, PageLocation pageLocation)
        {
            var matchingRazorComponents = new List<Type>();
            foreach(var pluginAssembly in ApplicationInfo.loadedPlugins.Select(p=>p.Assembly))
            {
                var renderFragmentComponents = pluginAssembly.GetPluginComponents();
                if (renderFragmentComponents.Count() > 0)
                {
                    foreach(var renderFragmentComponent in renderFragmentComponents)
                    {
                        var attributeData = renderFragmentComponent.GetPluginComponentAttributes();
                        foreach(var attribute in attributeData)
                        {
                            if (attribute.PageType == pageType && attribute.PageLocation == pageLocation)
                            {
                                matchingRazorComponents.Add(renderFragmentComponent);
                            }
                        }
                    }
                }
            }
            return matchingRazorComponents;
        }
        public static Type? GetPluginSettingsComponent(IPluginBase plugin)
        {
            var matchingPlugin = ApplicationInfo.loadedPlugins.FirstOrDefault(p => p.PluginBase.Equals(plugin));
            if (matchingPlugin.Assembly != null)
            {
                var settingsPage = matchingPlugin.Assembly.GetPluginComponents();
                foreach (Type pluginRenderFragment in settingsPage)
                {
                    var attribute = pluginRenderFragment.GetPluginComponentAttributes().FirstOrDefault();
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
