using BLAZAM.Logger;
using Microsoft.EntityFrameworkCore;

namespace BLAZAM.Database.Context
{
    public class SqlitPluginDbConetext : DbContext
    {


        public SqlitPluginDbConetext() : base()
        {
        }

        public SqlitPluginDbConetext(string databaseConnectionString) : base()
        {
            databaseConnectionString = databaseConnectionString ?? "Data Source=database.db";
        }

        public SqlitPluginDbConetext(DbContextOptions options) : base(options)
        {
        }




        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(
                           "Data Source=database.db").EnableSensitiveDataLogging()
                            .LogTo(Loggers.DatabaseLogger.Information);
        }

    }
}
