using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Plugins
{
    /// <summary>
    /// Defines a contract for injecting services into a <see cref="WebApplicationBuilder"/> instance.
    /// </summary>
    /// <remarks>Implementations of this interface are responsible for configuring and adding services to the
    /// application's dependency injection container. This is typically used to modularize service registration for
    /// plugins or extensions.</remarks>
    public interface IPluginServiceProvider
    {
        WebApplicationBuilder InjectServices(WebApplicationBuilder builder);
    }
}
