using System.Data;
using System.Data.SQLite;
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



        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.HResult == -2147467259)
                {
                    await Task.Delay(500);
                    return await RetrySaveChangesAsync(cancellationToken);
                }
                return -1;

            }
            catch (Exception ex)
            {
                LastSaveError = ex.InnerException ?? ex;
                return -1;
            }
        }
        private async Task<int> RetrySaveChangesAsync(CancellationToken cancellationToken = default, int retries = 0)
        {
            try
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                if (retries < 15 && ex.InnerException?.HResult == -2147467259)
                {
                    await Task.Delay(500);
                    return await RetrySaveChangesAsync(cancellationToken, retries + 1);
                }
                return -1;

            }
            catch (Exception ex)
            {
                LastSaveError = ex.InnerException ?? ex;
                return -1;
            }
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
