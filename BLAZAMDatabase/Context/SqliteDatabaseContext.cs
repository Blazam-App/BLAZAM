using BLAZAM.Common.Data.Database;
using BLAZAM.Logger;
using Microsoft.EntityFrameworkCore;
namespace BLAZAM.Database.Context
{
    public class SqliteDatabaseContext : DatabaseContextBase
    {
        

        public SqliteDatabaseContext() : base()
        {
        }

        public SqliteDatabaseContext(DatabaseConnectionString databaseConnectionString) : base(databaseConnectionString)
        {
        }

        public SqliteDatabaseContext(DbContextOptions options) : base(options)
        {
        }





        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (ConnectionString == null)
            {
                ConnectionString = new DatabaseConnectionString("database.db");

            }
            optionsBuilder.UseSqlite(
                         ConnectionString.Value).EnableSensitiveDataLogging()
                          .LogTo(Loggers.DatabaseLogger.Information);
        }


    }
}
