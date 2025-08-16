using BLAZAM.Global.Data;
using BLAZAM.Plugins;
using Microsoft.AspNetCore.Builder;

namespace BLAZAPluginTest
{
    public class BlazamTestPlugin : IPluginBase, IPluginServiceProvider
    {


        public string Name => "Example Plugin";

        public PluginVersion Version => new("1.0.0");

        public string Author => "Blazam";

        public static Guid Guid => Guid.Parse("e2a1c7b2-4f8e-4c3a-9b7e-2d8f1a6e5c3f");

        public WebApplicationBuilder InjectServices(WebApplicationBuilder builder)
        {
            return builder;
        }
    }
}
