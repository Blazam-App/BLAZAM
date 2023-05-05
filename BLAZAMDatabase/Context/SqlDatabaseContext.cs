using BLAZAM.Common.Data.Database;
using BLAZAM.Logger;
using Microsoft.EntityFrameworkCore;

namespace BLAZAM.Database.Context
{
    public class SqlDatabaseContext : DatabaseContextBase
    {
        

        public SqlDatabaseContext() : base()
        {
        }

        public SqlDatabaseContext(DatabaseConnectionString databaseConnectionString) : base(databaseConnectionString)
        {
        }

        public SqlDatabaseContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (ConnectionString == null)
            {
                ConnectionString = new DatabaseConnectionString("test");

            }
            optionsBuilder.UseSqlServer(
                       ConnectionString.Value,
                            sqlServerOptionsAction: sqlOptions =>
                            {
                                sqlOptions.EnableRetryOnFailure();

                            }
                                ).EnableSensitiveDataLogging()
                                .LogTo(Loggers.DatabaseLogger.Information);
        }


    }
}
