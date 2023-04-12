using BLAZAM.Common.Data.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BLAZAM.Database.Context
{

    public class AppDatabaseFactory
    {
        IConfiguration _configuration;

        public AppDatabaseFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IDatabaseContext> CreateDbContextAsync() => await Task.Run(async () => { return CreateDbContext(); });
        public IDatabaseContext CreateDbContext()
        {
            var _dbType = _configuration.GetValue<string>("DatabaseType");
            // Console.WriteLine("Database Type: " + _dbType);
            IDatabaseContext databaseContext = null;
            switch (_dbType.ToLower())
            {





                case "sql":
                    databaseContext = new SqlDatabaseContext(new DatabaseConnectionString(_configuration.GetConnectionString("SQLConnectionString"), DatabaseType.SQL));

                    break;
                case "sqlite":

                    databaseContext = new SqliteDatabaseContext(new DatabaseConnectionString(_configuration.GetConnectionString("SQLiteConnectionString"), DatabaseType.SQLite));
                    break;

                case "mysql":
                    databaseContext = new MySqlDatabaseContext(new DatabaseConnectionString(_configuration.GetConnectionString("MySQLConnectionString"), DatabaseType.MySQL));
                    break;

            }
            return databaseContext == null
                ? throw new Exception("Database Context is null. Attempted connection to a "
                + _dbType + " type database")
                : databaseContext;
        }
    }
}
