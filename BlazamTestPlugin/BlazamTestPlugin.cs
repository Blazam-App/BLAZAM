using BLAZAM.Database.Context;
using BLAZAM.Plugins;
using Microsoft.AspNetCore.Builder;

namespace BlazamTestPlugin
{

    public class BlazamTestPlugin : IPluginBase
    {
        private readonly IAppDatabaseFactory _appDatabaseFactory;


        public string Name => "Test Plugin";

        public string Version => "1.0";

        public string Author => "jacobsen9026";



        public WebApplicationBuilder InjectServices(WebApplicationBuilder builder)
        {
            return builder;
        }
    }

}
