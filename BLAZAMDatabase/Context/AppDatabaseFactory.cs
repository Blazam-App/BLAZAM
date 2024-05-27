using BLAZAM.Common.Data;
using BLAZAM.Common.Data.Database;
using BLAZAM.Database.Exceptions;
using BLAZAM.Database.Models.Permissions;
using BLAZAM.Logger;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net.WebSockets;

namespace BLAZAM.Database.Context
{
    /// <summary>
    /// A factory for dynamic creation of <see cref="DbContext"/> objects based on <see cref="IConfiguration"/> properties.
    /// </summary>
    public class AppDatabaseFactory : IAppDatabaseFactory
    {
        IConfiguration _configuration;

        public static DatabaseException DatabaseCreationFailureReason { get; set; }
        public static AppEvent? OnMigrationApplied { get; set; }
        public static AppEvent<Exception>? OnFatalError { get; set; }
        public static Exception? FatalError { get; private set; }

        /// <summary>
        /// Creates a new factory with the supplied configuration and <see cref="ApplicationInfo"/>
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="appInfo"></param>
        public AppDatabaseFactory(IConfiguration configuration)
        {
            _configuration = configuration;

            //Perform database auto update
            ApplyDatabaseMigrations();
            try
            {
                ApplicationInfo.installationCompleted = CheckInstallation();
            }
            catch (DatabaseException ex)
            {
                FatalError = ex;
                OnFatalError?.Invoke(ex);

            }
            SeedData();
            StartDatabaseCache();

        }
        /// <summary>
        /// Seeds any data that can't be covered in a migration
        /// </summary>
        /// <remarks>
        /// Each seed should have a check requirement to ensure it is needed
        /// </remarks>
        /// <exception cref="NotImplementedException"></exception>
        private void SeedData()
        {
            var seedContext = this.CreateDbContext();


            SetupDenyAll(seedContext);

        }

        private void SetupDenyAll(IDatabaseContext seedContext)
        {
            bool saveRequired = false;
            var denyAll = seedContext.AccessLevels.First(x => x.Id == 1);
            if (denyAll != null)
            {
                foreach (var adObjectType in Enum.GetValues(typeof(ActiveDirectoryObjectType)))
                {
                    if ((ActiveDirectoryObjectType)adObjectType != ActiveDirectoryObjectType.All)
                    {
                        if (denyAll.ObjectMap.Any(x => x.ObjectType == (ActiveDirectoryObjectType)adObjectType))
                        {
                            var eexisingObjectMap = denyAll.ObjectMap.First(x => x.ObjectType == (ActiveDirectoryObjectType)adObjectType);
                            if (eexisingObjectMap.ObjectAccessLevelId != ObjectAccessLevels.Deny.Id)
                            {
                                denyAll.ObjectMap.Remove(eexisingObjectMap);
                                saveRequired = true;
                            }
                        }
                        if (!denyAll.ObjectMap.Any(x => x.ObjectType == (ActiveDirectoryObjectType)adObjectType && x.ObjectAccessLevel.Id == ObjectAccessLevels.Deny.Id))
                        {
                            denyAll.ObjectMap.Add(new()
                            {
                                ObjectType = (ActiveDirectoryObjectType)adObjectType,
                                ObjectAccessLevelId = ObjectAccessLevels.Deny.Id,
                            });
                            saveRequired = true;

                        }
                    }
                }

            }
            if (saveRequired)
            {
                seedContext.SaveChanges();
            }
        }

        private void StartDatabaseCache()
        {
            //Start the database cache

            DatabaseCache.Start(this);

        }

        /// <summary>
        /// Checks that the DB can connect, has seed data, and has
        /// the installation flag set
        /// </summary>
        /// <returns>Returns true if all check pass</returns>
        /// <exception cref="DatabaseException"></exception>
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
                                //Grab the app settings to check that the install completed
                                //flag is set
                                var appSettings = context.AppSettings.FirstOrDefault();
                                if (appSettings != null)
                                    return appSettings.InstallationCompleted;
                                else
                                    return false;
                            }
                            catch (Exception ex)
                            {
                                Loggers.DatabaseLogger.Error("There was an error checking the installation flag in the database. {@Error}", ex);
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        throw new DatabaseException("The database could not be checked for installation.", ex);
                    }
                }

            }
            return false;
        }
        /// <summary>
        /// Async call to <see cref="CreateDbContext"/>
        /// </summary>
        /// <returns></returns>
        public async Task<IDatabaseContext> CreateDbContextAsync() => await Task.Run(() => { return CreateDbContext(); });

        /// <summary>
        /// Creates a new application <see cref="DbContext"/> based on the configured DatabaseType
        /// and DBConnectionString in appsettings.json
        /// <para>
        /// Currently supports SQL,MySQL/MariaDB, and SQlite
        /// </para>
        /// </summary>
        /// <remarks>
        /// Throws a <see cref="DatabaseException"/> when the attempt to connect fails
        /// </remarks>
        /// <returns>A valid and connected <see cref="IDatabaseContext"/></returns>
        /// <exception cref="DatabaseException"></exception>
        /// <exception cref="Exception">Thrown for unexpected exceptions</exception>
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

        public async Task<bool> ApplyDatabaseMigrationsAsync(bool force = false)
        {
            return await Task.Run(() =>
            {
                return ApplyDatabaseMigrations(force);
            });

        }
        public bool ApplyDatabaseMigrations(bool force = false)
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
                Loggers.DatabaseLogger.Error("Database Auto-Update Failed!!!! {@Error}", ex);
                throw ex;
            }



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
                Loggers.DatabaseLogger.Error("Database Auto-Update Failed!!!! {@Error}", ex);
                //DownReason = new DatabaseException(ex.Message, ex);
                FatalError = ex;
                OnFatalError?.Invoke(ex);
                return false;
            }
        }
    }
}
