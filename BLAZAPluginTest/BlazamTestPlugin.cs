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

        public IPluginView? SettingsPage => null;

        public WebApplicationBuilder InjectServices(WebApplicationBuilder builder)
        {
            return builder;
        }
    }
}
