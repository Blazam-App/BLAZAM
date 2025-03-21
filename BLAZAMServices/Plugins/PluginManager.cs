using BLAZAM.FileSystem;
using BLAZAM.Logger;
using BLAZAM.Plugins;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Org.BouncyCastle.Asn1.Cms;
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

      

    
        public void RegisterPluginComponents()
        {
            foreach (var pluginAssembly in ApplicationInfo.loadedPlugins)
            {
                // Scan for Razor components with PluginRenderFragmentAttribute
                var componentTypes = pluginAssembly.Key.DefinedTypes
                    .Where(t => t is PluginComponentBase);

                foreach (var componentType in componentTypes)
                {
                    try
                    {
                        var componentInstance = Activator.CreateInstance(componentType) as PluginComponentBase;

                        if (componentInstance != null)
                        {
                            
                        
                            var pageType = componentInstance.PageType;
                            Context.RegisterPluginComponent(pageType, componentType);
                            Console.WriteLine($"Registered render fragment {componentType.FullName} for location '{pageType}' from {pluginAssembly.Key.GetName().Name}");

                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error registering render fragment for {componentType.FullName} from {pluginAssembly.Key.GetName().Name}: {ex.Message}");
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
