using BLAZAM.Common.Data.Database;
using BLAZAM.Logger;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

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
        protected override DataTable SelectAllDataFromTable(string? tableName)
        {
            // Create a SQL query to select all rows from the table
            var query = $"SELECT * FROM {tableName}";

            // Create a data adapter to execute the query and fill a data table
            var adapter = new SqlDataAdapter(query, ConnectionString.Value);
            var table = new DataTable();
            adapter.Fill(table);
            return table;
        }

    }
}
