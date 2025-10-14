using System.Data;
using BLAZAM.Common.Data;
using BLAZAM.Common.Data.Database;
using BLAZAM.Common.Helpers;
using BLAZAM.Database.Exceptions;
using BLAZAM.Database.Models;
using BLAZAM.Database.Models.Audit;
using BLAZAM.Database.Models.Chat;
using BLAZAM.Database.Models.Notifications;
using BLAZAM.Database.Models.Permissions;
using BLAZAM.Database.Models.Rules;
using BLAZAM.Database.Models.Templates;
using BLAZAM.Database.Models.User;
using BLAZAM.FileSystem;
using BLAZAM.Global.Enums;
using BLAZAM.Helpers;
using BLAZAM.Logger;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;

namespace BLAZAM.Database.Context
{
    public class DatabaseContextBase : DbContext, IDatabaseContext
    {
        public Exception? LastSaveError { get; set; }






        public DatabaseConnectionString? ConnectionString { get; set; }


        public virtual ServiceConnectionState Status
        {
            get
            {
                return TestConnection();
            }
        }

        private IEnumerable<string> _pendingMigrations;
        public virtual IEnumerable<string> PendingMigrations
        {
            get
            {
                _pendingMigrations ??= Database.GetPendingMigrations();
                return _pendingMigrations;
            }
        }

        private IEnumerable<string> _appliedMigrations;



        public virtual IEnumerable<string> AppliedMigrations
        {
            get
            {

                _appliedMigrations ??= Database.GetAppliedMigrations();
                return _appliedMigrations;
            }
        }

        public enum DatabaseStatus
        {
            OK, ServerUnreachable,
            TablesMissing,
            DatabaseConnectionIssue,
            IncompleteConfiguration
        }
        /// <summary>
        /// Constructor for building migrations
        /// </summary>
        public DatabaseContextBase()
        {
            ConnectionString = new("");
            ApplicationStatistics.AddDBContext();

        }


        public DatabaseContextBase(DatabaseConnectionString databaseConnectionString) : base()
        {
            ConnectionString = databaseConnectionString;
            ApplicationStatistics.AddDBContext();

        }







        public DatabaseContextBase(DbContextOptions options) : base(options)
        {
            ApplicationStatistics.AddDBContext();

        }

        //Data tables
        public virtual DbSet<LockedOutUser> LockedOutUsers { get; set; }

        //App Settings
        public virtual DbSet<AppSettings> AppSettings { get; set; }
        public virtual DbSet<ADSettings> ActiveDirectorySettings { get; set; }
        public virtual DbSet<AuthenticationSettings> AuthenticationSettings { get; set; }
        public virtual DbSet<EmailSettings> EmailSettings { get; set; }
        public virtual DbSet<EmailTemplate> EmailTemplates { get; set; }
        public virtual DbSet<WebHookSubscription> WebHookSubscriptions { get; set; }
        public virtual DbSet<WebHookAttempt> WebHookAttempts { get; set; }

        //Autoation Rules
        public virtual DbSet<AutomationRule> AutomationRules { get; set; }
        public virtual DbSet<AutomationRuleActionFieldValue> AutomationRuleFieldValues { get; set; }
        public virtual DbSet<AutomationRuleOrFilter> AutomationRuleOrFilter { get; set; }
        public virtual DbSet<AutomationRuleAndFilter> AutomationRuleAndFilters { get; set; }
        public virtual DbSet<AutomationRuleGroupGuid> AutomationRuleGroupGuids { get; set; }
        public virtual DbSet<AutomationRuleAction> AutomationRuleActions { get; set; }


        //User Tables
        public virtual DbSet<AppUser> UserSettings { get; set; }
        public virtual DbSet<UserNotification> UserNotifications { get; set; }
        public virtual DbSet<ReadNewsItem> ReadNewsItems { get; set; }
        public virtual DbSet<UserFavoriteEntry> UserFavoriteEntries { get; set; }
        public virtual DbSet<UserDashboardWidget> UserDashboardWidgets { get; set; }
        public virtual DbSet<NotificationMessage> NotificationMessages { get; set; }
        public virtual DbSet<NotificationSubscription> NotificationSubscriptions { get; set; }
        public virtual DbSet<ApiToken> ApiTokens { get; set; }


        //Audit Logs
        public virtual DbSet<SystemAuditLog> SystemAuditLog { get; set; }
        public virtual DbSet<LogonAuditLog> LogonAuditLog { get; set; }
        public virtual DbSet<EmailAuditLog> EmailAuditLog { get; set; }
        public virtual DbSet<FailedADLogonEvent> FailedADLogonEvents { get; set; }
        public virtual DbSet<DirectoryEntryAuditLog> DirectoryEntryAuditLogs { get; set; }
        public virtual DbSet<RequestAuditLog> RequestAuditLog { get; set; }
        public virtual DbSet<PermissionsAuditLog> PermissionsAuditLog { get; set; }
        public virtual DbSet<SettingsAuditLog> SettingsAuditLog { get; set; }



        //Permissions
        public virtual DbSet<ActiveDirectoryField> ActiveDirectoryFields { get; set; }
        public virtual DbSet<CustomActiveDirectoryField> CustomActiveDirectoryFields { get; set; }
        public virtual DbSet<ActiveDirectoryFieldObjectType> ActiveDirectoryFieldObjectMappings { get; set; }
        public virtual DbSet<AccessLevel> AccessLevels { get; set; }
        public virtual DbSet<ObjectAccessMapping> AccessLevelObjectMapping { get; set; }
        public virtual DbSet<FieldAccessMapping> AccessLevelFieldMapping { get; set; }
        public virtual DbSet<FieldAccessLevel> FieldAccessLevel { get; set; }
        public virtual DbSet<ObjectAccessLevel> ObjectAccessLevel { get; set; }
        public virtual DbSet<ObjectAction> ObjectActionFlag { get; set; }

        public virtual DbSet<PermissionDelegate> PermissionDelegate { get; set; }
        public virtual DbSet<PermissionMapping> PermissionMap { get; set; }
        public virtual DbSet<GlobalPermissionSettings> GlobalPermissionSettings { get; set; }
        public virtual DbSet<GlobalAutomationRuleSettings> GlobalAutomationRuleSettings { get; set; }
        public virtual DbSet<GlobalPermissionRequestAction> GlobalPermissionRequestActions { get; set; }

        public virtual DbSet<ChatRoom> ChatRooms { get; set; }
        public virtual DbSet<ChatMessage> ChatMessages { get; set; }
        public virtual DbSet<UnreadChatMessage> UnreadChatMessages { get; set; }


        //Templates
        public virtual DbSet<DirectoryTemplate> DirectoryTemplates { get; set; }
        public virtual DbSet<DirectoryTemplateFieldValue> DirectoryTemplateFieldValues { get; set; }
        public virtual DbSet<DirectoryTemplateGroup> DirectoryTemplateGroups { get; set; }



        public static ConfigurationManager Configuration { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            throw new NotImplementedException("DatabaseContext of type " + GetType().FullName + " has not implemented OnConfiguring");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            DatabaseHelpers.ConfigureModel(modelBuilder, this);
        }




        public bool EntityIsTracked<TEntry>(TEntry? entry)
        {
            if (entry is null)
            {
                return false;
            }

            if (EqualityComparer<TEntry>.Default.Equals(entry, default))
            {
                return false;
            }

            return base.Entry(entry).State != EntityState.Detached;
        }

        public static DatabaseException DownReason { get; set; }




        /// <summary>
        /// This should be private
        /// </summary>
        /// <returns></returns>
        private ServiceConnectionState TestConnection()
        {
            if (ConnectionString == null)
            {
                return ServiceConnectionState.Down;
            }

            try
            {
                if (ConnectionString.FileBased)
                {
                    return TestSqliteConnection();
                }

                if (!NetworkTools.IsPortOpen(ConnectionString.GetServerAddress(), ConnectionString.GetServerPort()))
                {
                    DownReason = new("The database port is not open or is not reachable.");
                    Database.CloseConnection();
                    return ServiceConnectionState.Down;
                }

                if (Database.CanConnect())
                {
                    return ServiceConnectionState.Up;
                }

            }
            catch (ObjectDisposedException ex)
            {
                Loggers.DatabaseLogger.Information(ex, "Attempted to access a disposed Database object");
            }
            catch (InvalidOperationException ex)
            {
                Loggers.DatabaseLogger.Information(ex, "Attempted to access a Database object in an invalid way");
            }
            catch (SqlException ex)
            {
                HandleSqlException(ex);
            }
            catch (RetryLimitExceededException)
            {
                DownReason = new("The retry limit exceeded trying to connect to the database.");
            }
            catch (DatabaseConnectionStringException ex)
            {
                DownReason = new("The database connection string is malformed. " + ex.Message);
            }
            catch (AppException ex)
            {
                DownReason = new("The database experienced a general error. " + ex.Message);
            }
            catch (Exception ex)
            {
                Loggers.DatabaseLogger.Error(ex, "Unexpected error testing connection to database");
                DownReason = new("The database experienced an unexpected error. " + ex.Message);
            }

            return ServiceConnectionState.Down;
        }

        private ServiceConnectionState TestSqliteConnection()
        {
            if (ConnectionString?.File.Writable != true)
            {
                DownReason = new("The Sqlite database folder is not writable by the current server user.");
                return ServiceConnectionState.Down;
            }

            if (ConnectionString.File.Exists)
            {
                return ServiceConnectionState.Up;
            }

            return ServiceConnectionState.Down;
        }

        private static void HandleSqlException(SqlException ex)
        {
            switch (ex.Number)
            {
                case 53:
                    DownReason = new("The database port is open but connecting as an Sql server failed.");
                    break;
                case 208:
                    DownReason = new("The database is missing a table. It may be in a corrupt state.");
                    break;
                case 18456:
                    DownReason = new("The database server is reachable, but the database could not be found or the credentials provided do not have permission to the database.");
                    break;
            }
        }



        /// <summary>
        /// Checks if the database seed migration hase been applied
        /// </summary>
        /// <remarks>If the database cannot connect this method returns true</remarks>
        /// <returns>Returns true if the seed migration has been applied, or the database can't be reached, otherwise
        /// returns false.</returns>
        public virtual bool IsSeeded()
        {



            try
            {
                if (AppliedMigrations.Any())
                {
                    return true;
                }

                if (AuthenticationSettings.FirstOrDefault() == null)
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }



        /// <summary>
        /// Checks if there is an applied and pending seed, indicating the migration
        /// chain has been reseeded and the database needs to be wiped and reinstalled
        /// </summary>
        /// <remarks>If the database cannot connect this returns false</remarks>
        /// <returns>Returns true if the seed migration has been applied, otherwise
        /// returns false.</returns>
        public virtual bool SeedMismatch
        {
            get
            {
                if (!IsSeeded())
                {
                    return false;
                }

                var seedMismatch = false;
                PendingMigrations.ForEach(am =>
                {
                    if (am.Contains("seed", StringComparison.OrdinalIgnoreCase))
                    {
                        seedMismatch = true;
                    }
                });
                return seedMismatch;
            }
        }


        public void Export(string directory)
        {
            // Get all the DbSet properties of the context
            var dbSets = this.GetType().GetProperties().Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

            // Loop through each DbSet property
            foreach (var dbSet in dbSets)
            {
                // Get the entity type of the DbSet
                var entityType = dbSet.PropertyType.GetGenericArguments()[0];

                // Get the table name of the entity type
                var tableName = Model.FindEntityType(entityType)?.GetTableName();

                DataTable table = SelectAllDataFromTable(tableName);

                // Create a CSV file name for the table
                var fileName = Path.Combine(directory, tableName + ".csv");
                var file = new SystemFile(fileName);
                file.EnsureCreated();
                // Write the data table to the CSV file
                using var writer = new StreamWriter(fileName);
                // Write the column names
                var columnNames = table.Columns.Cast<DataColumn>().Select(c => c.ColumnName);
                writer.WriteLine(string.Join(",", columnNames));

                // Write the rows
                foreach (DataRow row in table.Rows)
                {
                    var fields = row.ItemArray.Select(f => f?.ToString());
                    List<string> lines = [];
                    foreach (var field in fields)
                    {
                        lines.Add('"' + field + '"');
                    }

                    writer.WriteLine(string.Join(",", lines));
                }
            }
        }

        protected virtual DataTable SelectAllDataFromTable(string? tableName)
        {
            throw new NotImplementedException("The SelectAllDataFromTable method has not been implemented");

        }

        public override void Dispose()
        {
            ApplicationStatistics.RemoveDBContext();
            base.Dispose();
        }

        public override ValueTask DisposeAsync()
        {
            ApplicationStatistics.RemoveDBContext();
            return base.DisposeAsync();
        }
    }
}
