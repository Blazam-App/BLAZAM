using BLAZAM.Database.Exceptions;

namespace BLAZAM.Database.Context
{
    /// <summary>
    /// The primary database factory for BLAZAM.
    /// Creates <see cref="IDatabaseContext"/> types 
    /// that can have a number of database type backings
    /// </summary>
    public interface IAppDatabaseFactory
    {
        /// <summary>
        /// Applies any pending database migrations to the database synchronously
        /// </summary>
        /// <remarks>
        /// You must use true for <paramref name="force"/> if you want to apply migrations to an empty database
        /// </remarks>
        /// <param name="force">Force the update even if the database is empty and has not been seeded yet</param>
        /// <returns></returns>
        bool ApplyDatabaseMigrations(bool force = false);

        /// <summary>
        /// Applies any pending database migrations to the database asynchronously
        /// </summary>
        /// <remarks>
        /// You must use true for <paramref name="force"/> if you want to apply migrations to an empty database
        /// </remarks>
        /// <param name="force">Force the update even if the database is empty and has not been seeded yet</param>
        /// <returns></returns>
        Task<bool> ApplyDatabaseMigrationsAsync(bool force = false);
        /// <summary>
        /// Creates a new connection to this database
        /// </summary>
        /// <returns>A new parallel connection</returns>
        IDatabaseContext CreateDbContext();
        /// <summary>
        /// Creates a new connection to this database asynchronously
        /// </summary>
        /// <returns>A Task that will return a new parallel connection</returns>
        Task<IDatabaseContext> CreateDbContextAsync();
    }
}