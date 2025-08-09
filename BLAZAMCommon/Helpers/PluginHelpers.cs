using System.Reflection;

namespace BLAZAM.Helpers
{
    public static class PluginHelpers
    {
        public static IEnumerable<Type> GetPluginTypes(this Assembly assembly, Type pluginClassType)
        {

            // Find all types in the loaded assembly that implement IPluginBase
            return assembly.GetTypes()
                .Where(type => pluginClassType.IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);
        }
    }
}
