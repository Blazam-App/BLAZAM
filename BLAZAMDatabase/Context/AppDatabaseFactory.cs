using BLAZAM.Common.Data;
using BLAZAM.Common.Data.Database;
using BLAZAM.Database.Exceptions;
using BLAZAM.Logger;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BLAZAM.Database.Context
{

    public class AppDatabaseFactory : IAppDatabaseFactory
    {
        IConfiguration _configuration;


        public static AppEvent OnMigrationApplied { get; set; }
        public static AppEvent OnMigrationFailed { get; set; }


        public AppDatabaseFactory(IConfiguration configuration, ApplicationInfo appInfo)
        {
            _configuration = configuration;

            //Perform database auto update
            ApplyDatabaseMigrations();

            appInfo.InstallationCompleted = CheckInstallation();

        }

        private bool CheckInstallation()
        {
            using (var context = CreateDbContext())
            {
                if (context != null)
                {
                    if (context.IsSeeded())
                    {
                        try
                        {
                            var appSettings = context.AppSettings.FirstOrDefault();
                            if (appSettings != null)
                                return appSettings.InstallationCompleted;
                            else
                                return false;
                        }
                        catch (Exception ex)
                        {
                            Loggers.DatabaseLogger.Error("There was an error checking the installation flag in the database.", ex);
                        }
                        //if (!context.Seeded()) installationCompleted = false;
                        //else installationCompleted = (DatabaseCache.ApplicationSettings?.InstallationCompleted == true);
                    }

                }

            }
            return false;
        }

        public async Task<IDatabaseContext> CreateDbContextAsync() => await Task.Run(async () => { return CreateDbContext(); });
        public IDatabaseContext CreateDbContext()
        {
            var _dbType = _configuration.GetValue<string>("DatabaseType");
            // Console.WriteLine("Database Type: " + _dbType);
            IDatabaseContext databaseContext = null;
            switch (_dbType.ToLower())
            {





                case "sql":
                    databaseContext = new SqlDatabaseContext(new DatabaseConnectionString(_configuration.GetConnectionString("SQLConnectionString"), DatabaseType.SQL));

                    break;
                case "sqlite":

                    databaseContext = new SqliteDatabaseContext(new DatabaseConnectionString(_configuration.GetConnectionString("SQLiteConnectionString"), DatabaseType.SQLite));
                    break;

                case "mysql":
                    databaseContext = new MySqlDatabaseContext(new DatabaseConnectionString(_configuration.GetConnectionString("MySQLConnectionString"), DatabaseType.MySQL));
                    break;

            }
            return databaseContext == null
                ? throw new Exception("Database Context is null. Attempted connection to a "
                + _dbType + " type database")
                : databaseContext;
        }


        public async Task<bool> ApplyDatabaseMigrations(bool force = false)
        {

            return await Task.Run(() =>
            {
                try
                {
                    using (var context = CreateDbContext())
                    {
                        if (context != null && context.Status == ServiceConnectionState.Up)
                            if (context.IsSeeded() || force)
                                if (!context.SeedMismatch)
                                {
                                    if (context.Database.GetPendingMigrations().Count() > 0)
                                        Migrate(context);
                                }
                                else
                                {
                                    throw new DatabaseException("Database incompatible with current application version.");
                                }
                        //context.Database.Migrate();


                        return true;
                    }
                }
                catch (DatabaseException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    Loggers.DatabaseLogger.Error("Database Auto-Update Failed!!!!", ex);
                    throw ex;
                    return false;
                }
            });


        }



        /// <summary>
        ///     Applies any pending migrations for the context to the database. Will create the database
        ///     if it does not already exist.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///        If the migration fails, the <see cref="Status"/> will be set to <see cref="ServiceConnectionState.Down"/>
        ///        </para>
        ///     <para>
        ///         See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
        ///     </para>
        /// </remarks>
        /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>

        protected virtual bool Migrate(IDatabaseContext context)
        {
            try
            {
                context.Database.Migrate();
                return true;
            }
            catch (Exception ex)
            {
                Loggers.DatabaseLogger.Error("Database Auto-Update Failed!!!!", ex);
                //DownReason = new DatabaseException(ex.Message, ex);
                OnMigrationFailed?.Invoke();
                return false;
            }
        }
    }
}
