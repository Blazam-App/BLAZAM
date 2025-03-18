using Microsoft.AspNetCore.Builder;
using System.Reflection;

namespace BLAZAM.Plugins
{

    public interface IPluginBase
    {
        string Name { get; }
        string Version { get; }
        string Author { get; }



        WebApplicationBuilder InjectServices(WebApplicationBuilder builder);

        Assembly Assembly { get; set; }
    }
}
