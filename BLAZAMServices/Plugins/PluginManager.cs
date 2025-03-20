using BLAZAM.FileSystem;
using BLAZAM.Logger;
using BLAZAM.Plugins;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Services.Plugins
{
    public class PluginManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        public PluginContext Context = new PluginContext();
        private readonly List<IPluginBase> _loadedPlugins = new();

        public PluginManager(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        public IEnumerable<IPluginBase> LoadedPlugins => _loadedPlugins;

      

    
        private void RegisterRenderFragments()
        {
            foreach (var pluginAssembly in ApplicationInfo.loadedPlugins)
            {
                // Scan for Razor components with PluginRenderFragmentAttribute
                var componentTypes = pluginAssembly.GetTypes()
                    .Where(t => typeof(ComponentBase).IsAssignableFrom(t) && t.GetCustomAttribute<PluginRenderFragmentAttribute>() != null);

                foreach (var componentType in componentTypes)
                {
                    try
                    {
                        var attribute = componentType.GetCustomAttribute<PluginRenderFragmentAttribute>();
                        if (attribute != null)
                        {
                            var pageType = attribute.PageType;
                            Context.RegisterPluginComponent(pageType, componentType);
                            Console.WriteLine($"Registered render fragment {componentType.FullName} for location '{pageType}' from {pluginAssembly.GetName().Name}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error registering render fragment for {componentType.FullName} from {pluginAssembly.GetName().Name}: {ex.Message}");
                    }
                }
            }
        }

        public IEnumerable<Type> GetRenderFragments(PageType locationName)
        {
            return Context.GetPluginComponents(locationName);
        }

        public TData ApplyDataManipulation<TData>(string handlerName, TData data)
        {
            return Context.ApplyDataManipulation(handlerName, data);
        }
    }
}
