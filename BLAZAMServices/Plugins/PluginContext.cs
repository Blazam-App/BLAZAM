using BLAZAM.Plugins;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Services.Plugins
{

    public class PluginContext
    {
        private readonly Dictionary<PageType, List<Type>> _fragmentRegistrations = new();
        private readonly Dictionary<string, Func<object, object>> _dataManipulationHandlers = new();

        // Method to register a RenderFragment for a specific location
        public void RegisterPluginComponent(PageType pageType, Type comopnentType)
        {
            if (!_fragmentRegistrations.ContainsKey(pageType))
            {
                // Combine fragments by creating a new delegate that invokes both
                var existingFragments = _fragmentRegistrations[pageType];
                _fragmentRegistrations[pageType].Add(comopnentType);
            }
            else
            {
                _fragmentRegistrations.Add(pageType, new List<Type>() { comopnentType });
            }
        }

        // Method to get all registered RenderFragments for a location
        public IEnumerable<Type> GetPluginComponents(PageType pageType)
        {
            return _fragmentRegistrations.TryGetValue(pageType, out var fragments) ? fragments : Enumerable.Empty<Type>();
        }

        // Method to register a data manipulation handler for a specific type and name
        public void RegisterDataManipulationHandler<TData>(string handlerName, Func<TData, TData> handler)
        {
            if (_dataManipulationHandlers.ContainsKey(handlerName))
            {
                // Handle potential overwriting or combining of handlers if needed
                Console.WriteLine($"Warning: Data manipulation handler '{handlerName}' already registered. Overwriting.");
            }
            _dataManipulationHandlers[handlerName] = (data) => handler((TData)data);
        }

        // Method to apply data manipulation handlers for a specific name and data
        public TData ApplyDataManipulation<TData>(string handlerName, TData data)
        {
            if (_dataManipulationHandlers.TryGetValue(handlerName, out var handler))
            {
                try
                {
                    return (TData)handler(data);
                }
                catch (InvalidCastException)
                {
                    Console.WriteLine($"Error: Could not cast data to expected type for handler '{handlerName}'.");
                }
            }
            return data;
        }

        // Optional: Method to clear all registrations (useful for testing or reloading)
        public void ClearRegistrations()
        {
            _fragmentRegistrations.Clear();
            _dataManipulationHandlers.Clear();
        }
    }

}
