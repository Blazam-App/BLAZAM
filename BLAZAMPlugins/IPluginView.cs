using Microsoft.AspNetCore.Components;

namespace BLAZAM.Plugins
{
    public interface IPluginView
    {
        RenderFragment? View { get; }
        
    }
}
