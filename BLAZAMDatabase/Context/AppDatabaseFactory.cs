using BLAZAM.Common.Data;
using BLAZAM.Common.Data.Database;
using BLAZAM.Database.Exceptions;
using BLAZAM.Database.Models.Permissions;
using BLAZAM.Global.Enums;
using BLAZAM.Logger;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BLAZAM.Database.Context
{
    /// <summary>
    /// A factory for dynamic creation of <see cref="DbContext"/> objects based on <see cref="IConfiguration"/> properties.
    /// </summary>
    public class AppDatabaseFactory : IAppDatabaseFactory
    {
        private readonly IConfiguration _configuration;

        public static DatabaseException DatabaseCreationFailureReason { get; set; }
        public static AppDelegate? OnMigrationApplied { get; set; }
        public static AppDelegate<Exception>? OnFatalError { get; set; }
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
                Loggers.InstallationCompleted = ApplicationInfo.installationCompleted;
                if (ApplicationInfo.installationCompleted)
                {
                    SeedData();
                }

            }
            catch (DatabaseException ex)
            {
                FatalError = ex;
                OnFatalError?.Invoke(ex);

            }
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

            SetupDenyAll();



        }



        private void SetupDenyAll()
        {
            Task.Run(() =>
            {
                try
                {
                    using var seedContext = this.CreateDbContext();

                    // Use FirstOrDefault to safely retrieve the entity and prevent exceptions.
                    var denyAll = seedContext.AccessLevels.FirstOrDefault(x => x.Id == 1);
                    if (denyAll is null)
                    {
                        Loggers.DatabaseLogger.Warning("AccessLevel with Id=1 not found for DenyAll setup.");
                        return; // Exit if the required entity isn't found.
                    }

                    // Ensure the DenyAll access level is set up correctly.
                    bool wasModified = this.EnsureDenyPermissionsAreSet(denyAll);

                    if (wasModified)
                    {
                        seedContext.SaveChanges();
                    }
                }
                catch (SqlException ex)
                {
                    Loggers.DatabaseLogger.Warning(ex, "SQL error while attempting to seed DenyAll settings.");
                }
                catch (Exception ex)
                {
                    Loggers.DatabaseLogger.Error(ex, "Unexpected error while attempting to seed DenyAll settings.");
                }
            });
        }

        /// <summary>
        /// Verifies and applies 'Deny' permissions for all relevant object types.
        /// </summary>
        /// <param name="denyAll">The access level to configure.</param>
        /// <returns>A boolean indicating if the entity was modified.</returns>
        private bool EnsureDenyPermissionsAreSet(AccessLevel denyAll)
        {
            bool wasModified = false;

            // Process all Active Directory object types except for the general 'All' type.
            var objectTypesToProcess = Enum.GetValues<ActiveDirectoryObjectType>()
                .Where(type => type != ActiveDirectoryObjectType.All);

            foreach (var objectType in objectTypesToProcess)
            {
                // Find and remove any existing map for this type that is not set to 'Deny'.
                var incorrectMap = denyAll.ObjectMap
                    .FirstOrDefault(m => m.ObjectType == objectType && m.ObjectAccessLevelId != ObjectAccessLevels.Deny.Id);

                if (incorrectMap is not null)
                {
                    denyAll.ObjectMap.Remove(incorrectMap);
                    wasModified = true;
                }

                // After cleanup, check if a 'Deny' permission map exists.
                bool hasDenyPermission = denyAll.ObjectMap
                    .Any(m => m.ObjectType == objectType && m.ObjectAccessLevelId == ObjectAccessLevels.Deny.Id);

                // If a 'Deny' permission is missing, add it.
                if (!hasDenyPermission)
                {
                    denyAll.ObjectMap.Add(new()
                    {
                        ObjectType = objectType,
                        ObjectAccessLevelId = ObjectAccessLevels.Deny.Id,
                    });
                    wasModified = true;
                }
            }

            return wasModified;
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
            using var context = CreateDbContext();
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
                            {
                                return appSettings.InstallationCompleted;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        catch (Exception ex)
                        {
                            Loggers.DatabaseLogger.Error(ex, "There was an error checking the installation flag in the database.");
                        }

                    }
                }
                catch (Exception ex)
                {
                    throw new DatabaseException("The database could not be checked for installation.", ex);
                }
            }
            return false;
        }
        /// <summary>
        /// Async call to <see cref="CreateDbContext"/>
        /// </summary>
        /// <returns></returns>
        public async Task<IDatabaseContext> CreateDbContextAsync()
        {
            return await Task.Run(() => { return CreateDbContext(); });
        }
        public DatabaseType DatabaseType
        {
            get
            {
                var _dbType = _configuration.GetValue<string>("DatabaseType");
                if (_dbType == null)
                {
                    throw new DatabaseException("DatabaseType missing in configuration file");
                }

                switch (_dbType.ToLower())
                {

                    case "sql":
                        return DatabaseType.SQL;
                    case "sqlite":

                        return DatabaseType.SQLite;

                    case "mysql":
                        return DatabaseType.MySQL;

                }
                return DatabaseType.SQLite;
            }
        }


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

            IDatabaseContext? databaseContext = null;
            switch (DatabaseType)
            {

                case DatabaseType.SQL:
                    databaseContext = new SqlDatabaseContext(new DatabaseConnectionString(_configuration.GetConnectionString("DBConnectionString"), DatabaseType.SQL));

                    break;
                case DatabaseType.SQLite:

                    databaseContext = new SqliteDatabaseContext(new DatabaseConnectionString(_configuration.GetConnectionString("DBConnectionString"), DatabaseType.SQLite));
                    break;

                case DatabaseType.MySQL:
                    databaseContext = new MySqlDatabaseContext(new DatabaseConnectionString(_configuration.GetConnectionString("DBConnectionString"), DatabaseType.MySQL));
                    break;

            }
            return databaseContext == null
                ? throw new DatabaseException("Database Context is null. Attempted connection to a "
                + DatabaseType + " type database")
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
                using var context = CreateDbContext();
                if (context != null
                    && context.Status == ServiceConnectionState.Up
                    && (context.IsSeeded() || force))
                {
                    if (!context.SeedMismatch)
                    {
                        var pendingMigrations = context.Database.GetPendingMigrations();
                        if (pendingMigrations.Count() > 0)
                        {
                            Migrate(context);
                        }
                    }
                    else
                    {
                        throw new DatabaseException("Database incompatible with current application version.");
                    }
                }

                return true;
            }
            catch (DatabaseException ex)
            {
                OnFatalError?.Invoke(ex);
                Loggers.DatabaseLogger.Information(ex, "Fatal Database Error");
                FatalError = ex;
                throw;
            }
            catch (Exception ex)
            {
                Loggers.DatabaseLogger.Fatal(ex, "Database Auto-Update Failed!!!!");
                throw;
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
                Loggers.DatabaseLogger.Fatal(ex, "Database Auto-Update Failed!!!!");

                FatalError = ex;
                OnFatalError?.Invoke(ex);
                return false;
            }
        }
    }
}
