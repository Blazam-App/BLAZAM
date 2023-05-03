using BLAZAM.Common.Data.Database;
using BLAZAM.Logger;
using Microsoft.EntityFrameworkCore;

namespace BLAZAM.Database.Context
{
    public class MySqlDatabaseContext : DatabaseContextBase
    {
        
        public MySqlDatabaseContext() : base()
        {
            ConnectionString = new("server=localhost");
        }

        public MySqlDatabaseContext(DatabaseConnectionString databaseConnectionString) : base(databaseConnectionString)
        {
        }

        public MySqlDatabaseContext(DbContextOptions options) : base(options)
        {
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (ConnectionString == null)
            {
                ConnectionString = new DatabaseConnectionString("Server=localhost;User=blazam;Password=blazam;Database=blazam;");
            }
            optionsBuilder.UseMySql(ConnectionString?.Value,
                           serverVersion: new MySqlServerVersion(new Version(8, 0, 32)),
                          mySqlOptionsAction: options =>
                          {
                              options.EnableRetryOnFailure();
                              //options.SetSqlModeOnOpen();

                          })

                          .EnableSensitiveDataLogging()
                                  .LogTo(Loggers.DatabaseLogger.Information);
        }


    }
}
