using BLAZAM.Database.Context;
using BLAZAM.Plugins;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAPluginTest
{
    public class BlazamTestPlugin : IPluginBase
    {
        private readonly IAppDatabaseFactory _appDatabaseFactory;


        public string Name => "Test Plugin";

        public string Version => "1.0";

        public string Author => "jacobsen9026";

        public IPluginView? SettingsPage => null;


        public WebApplicationBuilder InjectServices(WebApplicationBuilder builder)
        {
            return builder;
        }
    }
}
