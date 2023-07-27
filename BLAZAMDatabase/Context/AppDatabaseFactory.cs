using BLAZAM.Common.Data;
using BLAZAM.Common.Data.Database;
using BLAZAM.Database.Exceptions;
using BLAZAM.Logger;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Runtime.CompilerServices;

namespace BLAZAM.Database.Context
{

    public class AppDatabaseFactory : IAppDatabaseFactory
    {
        IConfiguration _configuration;

        public static DatabaseException DatabaseCreationFailureReason { get; set; }
        public static AppEvent? OnMigrationApplied { get; set; }
        public static AppEvent<Exception>? OnFatalError { get; set; }
        public static Exception? FatalError { get; private set; }

        public AppDatabaseFactory(IConfiguration configuration, ApplicationInfo appInfo)
        {
            _configuration = configuration;

            //Perform database auto update
            ApplyDatabaseMigrations();
            try
            {
                appInfo.InstallationCompleted = CheckInstallation();
            }
            catch (DatabaseException ex)
            {
                FatalError = ex;
                OnFatalError?.Invoke(ex);

            }
            StartDatabaseCache();

        }
        private  void StartDatabaseCache()
        {
            //Start the database cache
            
                DatabaseCache.Start(this);
           
        }
        private bool CheckInstallation()
        {
            using (var context = CreateDbContext())
            {
                if (context != null)
                {
                    try
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
                            
                        }
                    }catch (Exception ex)
                    {
                        throw new DatabaseException("The database could not be checked for installation.", ex);
                    }
                }

            }
            return false;
        }

        public async Task<IDatabaseContext> CreateDbContextAsync() => await Task.Run(() => { return CreateDbContext(); });
       
        public IDatabaseContext CreateDbContext()
        {
            var _dbType = _configuration.GetValue<string>("DatabaseType");
            if (_dbType == null) throw new DatabaseException("DatabaseType missing in configuration file");
            // Console.WriteLine("Database Type: " + _dbType);
            IDatabaseContext? databaseContext = null;
            switch (_dbType.ToLower())
            {

                case "sql":
                    databaseContext = new SqlDatabaseContext(new DatabaseConnectionString(_configuration.GetConnectionString("DBConnectionString"), DatabaseType.SQL));

                    break;
                case "sqlite":

                    databaseContext = new SqliteDatabaseContext(new DatabaseConnectionString(_configuration.GetConnectionString("DBConnectionString"), DatabaseType.SQLite));
                    break;

                case "mysql":
                    databaseContext = new MySqlDatabaseContext(new DatabaseConnectionString(_configuration.GetConnectionString("DBConnectionString"), DatabaseType.MySQL));
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
                    OnFatalError?.Invoke(ex);
                    FatalError = ex;
                    throw ex;
                }
                catch (Exception ex)
                {
                    Loggers.DatabaseLogger.Error("Database Auto-Update Failed!!!!", ex);
                    throw ex;
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
                FatalError = ex;
                OnFatalError?.Invoke(ex);
                return false;
            }
        }
    }
}
