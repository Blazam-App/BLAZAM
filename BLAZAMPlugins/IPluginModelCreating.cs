using Microsoft.EntityFrameworkCore;

namespace BLAZAM.Plugins
{
    /// <summary>
    /// Implementing this interface allows a plugin to customize the database model
    /// by adding its own entity configurations to the application's <see cref="DbContext"/>.
    /// </summary>
    public interface IPluginModelCreating
    {
        /// <summary>
        /// Called when the database model for the application is being created.
        /// Use this method to configure your plugin's entities.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for the context.</param>
        void OnModelCreating(ModelBuilder modelBuilder);
    }
}
