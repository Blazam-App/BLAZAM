using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using BLAZAM.Database.Context;

namespace BLAZAM.Plugins
{

    public interface IPluginBase
    {
        string Name { get; }
        string Version { get; }
        string Author { get; }



        WebApplicationBuilder InjectServices(WebApplicationBuilder builder);
     }
}
