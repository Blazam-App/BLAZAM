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
        /// <inheritdoc/>

        public SqliteDatabaseContext():base()
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

            optionsBuilder.UseSqlite(
                         ConnectionString.Value).EnableSensitiveDataLogging()
                          .LogTo(Loggers.DatabaseLogger.Information);
        }

        
    }
}
