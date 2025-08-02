using BLAZAM.Global.Data;
using BLAZAM.Plugins;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
