using System;
using Microsoft.EntityFrameworkCore;

namespace BLAZAM.Plugins
{
    /// <summary>
    /// Implementing this interface allows a plugin to integrate its own <see cref="DbContext"/>
    /// with the application's main database.
    /// </summary>
    public interface IPluginDbContext
    {
        /// <summary>
        /// Gets the type of the plugin's <see cref="DbContext"/>.
        /// This context should be configured to use the same database provider as the main application.
        /// </summary>
        Type DbContextType { get; }
    }
}
