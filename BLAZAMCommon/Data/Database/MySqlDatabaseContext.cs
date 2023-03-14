using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Common.Data.Database
{
    public class MySqlDatabaseContext:DatabaseContext
    {
        public MySqlDatabaseContext(DatabaseConnectionString databaseConnectionString):base(databaseConnectionString)
        {
        }

        public MySqlDatabaseContext(DbContextOptions options) : base(options)
        {
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
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
