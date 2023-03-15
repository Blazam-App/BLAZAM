using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Common.Data.Database
{
    public class SqliteDatabaseContext:DatabaseContext
    {
        public SqliteDatabaseContext()
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
                ConnectionString = new DatabaseConnectionString("test");

            }
            optionsBuilder.UseSqlite(
                         ConnectionString.Value).EnableSensitiveDataLogging()
                          .LogTo(Loggers.DatabaseLogger.Information);
        }


    }
}
