using BLAZAM.Common.Data.Database;
using BLAZAM.Logger;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Data;
using System.Data.SQLite;

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
        protected override DataTable SelectAllDataFromTable(string? tableName)
        {
            // Create a SQLite query to select all rows from the table
            var query = $"SELECT * FROM {tableName}";

            // Create a data adapter to execute the query and fill a data table
            var adapter = new SQLiteDataAdapter(query, ConnectionString.Value);
            var table = new DataTable();
            adapter.Fill(table);
            return table;
        }

    }
}
