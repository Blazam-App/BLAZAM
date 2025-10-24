using BLAZAM.Common.Data.Database;

namespace BLAZAM.Database.Context
{
    public class UserDatabaseFactory : IUserDatabaseFactory, IDisposable
    {
        private readonly IAppDatabaseFactory _appDatabaseFactory;

        public UserDatabaseFactory(IAppDatabaseFactory appDatabaseFactory)
        {
            _appDatabaseFactory = appDatabaseFactory;
        }

        public DatabaseType DatabaseType => _appDatabaseFactory.DatabaseType;

        public bool ApplyDatabaseMigrations(bool force = false) => _appDatabaseFactory.ApplyDatabaseMigrations(force);

        public Task<bool> ApplyDatabaseMigrationsAsync(bool force = false) => _appDatabaseFactory.ApplyDatabaseMigrationsAsync(force);

        private List<IDatabaseContext> _userContexts = [];

        public IDatabaseContext CreateDbContext()
        {
            var ctx = _appDatabaseFactory.CreateDbContext();
            _userContexts.Add(ctx);

            return ctx;
        }

        public async Task<IDatabaseContext> CreateDbContextAsync()
        {
            var ctx = await _appDatabaseFactory.CreateDbContextAsync();
            _userContexts.Add(ctx);

            return ctx;
        }

        public void Dispose()
        {
            foreach (var ctx in _userContexts)
            {
                ctx.Dispose();
            }
        }
    }
}
