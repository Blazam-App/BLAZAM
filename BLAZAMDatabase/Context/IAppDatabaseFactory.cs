namespace BLAZAM.Database.Context
{
    public interface IAppDatabaseFactory
    {
        IDatabaseContext CreateDbContext();
        Task<IDatabaseContext> CreateDbContextAsync();
    }
}