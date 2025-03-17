using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using BLAZAM.Database.Context;
using Microsoft.AspNetCore.Components;

namespace BLAZAM.Plugins
{

    public interface IPluginBase
    {
        string Name { get; }
        string Version { get; }
        string Author { get; }



        WebApplicationBuilder InjectServices(WebApplicationBuilder builder);

            IPluginView? SettingsPage { get; }
    }
}
