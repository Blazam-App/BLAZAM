using BLAZAM.Database.Exceptions;

namespace BLAZAM.Database.Context
{
    public interface IAppDatabaseFactory
    {

        Task<bool> ApplyDatabaseMigrations(bool force = false);
        IDatabaseContext CreateDbContext();
        Task<IDatabaseContext> CreateDbContextAsync();
    }
}