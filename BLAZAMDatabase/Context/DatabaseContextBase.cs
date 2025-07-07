
using BLAZAM.Common.Data;
using BLAZAM.Common.Data.Database;
using BLAZAM.Common.Exceptions;
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
using BLAZAM.Helpers;
using BLAZAM.Localization;
using BLAZAM.Logger;
using BLAZAM.Server.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Reflection;

namespace BLAZAM.Database.Context
{
    public class DatabaseContextBase : DbContext, IDatabaseContext
    {
        public Exception? LastSaveError { get; set; }
        public override void Dispose()
        {
            ApplicationStatistics.RemoveDBContext();

            base.Dispose();
        }





        public DatabaseConnectionString? ConnectionString { get; set; }


        public virtual ServiceConnectionState Status
        {
            get
            {
                return TestConnection();
            }
        }

        private static IEnumerable<string> _pendingMigrations;
        public virtual IEnumerable<string> PendingMigrations
        {
            get
            {
                _pendingMigrations ??= Database.GetPendingMigrations();
                return _pendingMigrations;
            }
        }

        private static IEnumerable<string> _appliedMigrations;



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
        public virtual DbSet<GenericSidList> LockedOutUsers { get; set; }

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
        public virtual DbSet<AutomationRuleGroupSid> AutomationRuleGroupSids { get; set; }
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
            List<ActiveDirectoryField> activeDirectoryFields = typeof(ActiveDirectoryFields).GetStaticProperties<ActiveDirectoryField>();


            modelBuilder.Entity<ActiveDirectoryField>().HasData(activeDirectoryFields);


            modelBuilder.Entity<CustomActiveDirectoryField>()
         .HasMany(x => x.ObjectTypes);
            modelBuilder.Entity<CustomActiveDirectoryField>()
         .Navigation(x => x.ObjectTypes).AutoInclude();




            modelBuilder.Entity<AccessLevel>(entity =>
            {
                entity.HasData(
                        new AccessLevel { Id = 1, Name = "Deny All" }
                );
                entity.Navigation(e => e.ObjectMap).AutoInclude();
                entity.Navigation(e => e.FieldMap).AutoInclude();
                entity.Navigation(e => e.ActionMap).AutoInclude();
            });


            List<FieldAccessLevel> fieldAccessLevels = typeof(FieldAccessLevels).GetStaticProperties<FieldAccessLevel>();

            modelBuilder.Entity<FieldAccessLevel>().HasData(fieldAccessLevels);


            modelBuilder.Entity<FieldAccessMapping>(entity =>
            {
                entity.Navigation(e => e.CustomField).AutoInclude();
                entity.Navigation(e => e.Field).AutoInclude();
                entity.Navigation(e => e.FieldAccessLevel).AutoInclude();
            });

            List<ObjectAccessLevel> objectAccessLevels = typeof(ObjectAccessLevels).GetStaticProperties<ObjectAccessLevel>();

            modelBuilder.Entity<ObjectAccessLevel>(entity =>
            {
                entity.HasData(objectAccessLevels);
            });

            modelBuilder.Entity<ObjectAccessMapping>(entity =>
            {
                entity.Navigation(e => e.ObjectAccessLevel).AutoInclude();
            });

            modelBuilder.Entity<DirectoryTemplate>(entity =>
            {
                entity.Navigation(e => e.FieldValues).AutoInclude();
                entity.Navigation(e => e.AssignedGroupSids).AutoInclude();
            });

            modelBuilder.Entity<AutomationRule>(entity =>
            {
                entity.Navigation(e => e.Actions).AutoInclude();
                entity.Navigation(e => e.Filters).AutoInclude();
            });

            modelBuilder.Entity<AutomationRuleAction>(entity =>
            {
                entity.Navigation(e => e.GroupSids).AutoInclude();
                entity.Navigation(e => e.FieldValues).AutoInclude();
            });

            modelBuilder.Entity<AutomationRuleOrFilter>(entity =>
            {
                entity.Navigation(e => e.AndFilters).AutoInclude();
            });

            modelBuilder.Entity<AutomationRuleAndFilter>(entity =>
            {
                entity.Navigation(e => e.CustomField).AutoInclude();
                entity.Navigation(e => e.Field).AutoInclude();
                entity.Property(e => e.TimeFrame)
                .HasConversion(new ValueConverter<TimeSpan?, long?>(
                        v => v.HasValue ? v.Value.Ticks : (long?)null,
                        v => v.HasValue ? TimeSpan.FromTicks(v.Value) : (TimeSpan?)null
                    ));
            });

            modelBuilder.Entity<AutomationRuleActionFieldValue>(entity =>
            {
                entity.Navigation(e => e.Field).AutoInclude();
                entity.Navigation(e => e.CustomField).AutoInclude();
            });

            modelBuilder.Entity<DirectoryTemplateFieldValue>(entity =>
            {
                entity.Navigation(e => e.Field).AutoInclude();
                entity.Navigation(e => e.CustomField).AutoInclude();
            });


            List<ObjectAction> objectActions = typeof(ObjectActions).GetStaticProperties<ObjectAction>();

            modelBuilder.Entity<ObjectAction>().HasData(objectActions);

            modelBuilder.Entity<ActionAccessMapping>(entity =>
            {
                entity.Navigation(e => e.ObjectAction).AutoInclude();
            });

            modelBuilder.Entity<PermissionMapping>(entity =>
            {
                entity.Navigation(e => e.AccessLevels).AutoInclude();
            });

            modelBuilder.Entity<DirectoryTemplate>(entity =>
            {
                entity.Navigation(e => e.AssignedGroupSids).AutoInclude();
            });

            modelBuilder.Entity<AppSettings>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                if (Database.IsMySql())
                    entity.ToTable(t => t.HasCheckConstraint("CK_Table_Column", "Id = 1"));
                else
                    entity.ToTable(t => t.HasCheckConstraint("CK_Table_Column", "[Id] = 1"));
            });

            modelBuilder.Entity<ADSettings>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                if (Database.IsMySql())
                    entity.ToTable(t => t.HasCheckConstraint("CK_Table_Column", "Id = 1"));
                else
                    entity.ToTable(t => t.HasCheckConstraint("CK_Table_Column", "[Id] = 1"));

            });



            modelBuilder.Entity<AuthenticationSettings>(entity =>
               {
                   entity.Property(e => e.Id).ValueGeneratedNever();

                   if (Database.IsMySql())
                       entity.ToTable(t => t.HasCheckConstraint("CK_Table_Column", "Id = 1"));

                   else
                       entity.ToTable(t => t.HasCheckConstraint("CK_Table_Column", "[Id] = 1"));
                   entity.HasData(new AuthenticationSettings
                   {
                       Id = 1,
                       AdminPassword = "password"
                   });
               });

            modelBuilder.Entity<EmailSettings>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                if (Database.IsMySql())
                    entity.ToTable(t => t.HasCheckConstraint("CK_Table_Column", "Id = 1"));

                else
                    entity.ToTable(t => t.HasCheckConstraint("CK_Table_Column", "[Id] = 1"));

            });


            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.HasIndex(e => e.UserGUID).IsUnique();
                entity.Navigation(e => e.ReadNewsItems).AutoInclude();
                entity.Navigation(e => e.FavoriteEntries).AutoInclude();
                entity.Navigation(e => e.DashboardWidgets).AutoInclude();
            });

            modelBuilder.Entity<UserNotification>(entity =>
            {
                entity.Navigation(e => e.Notification).AutoInclude();
            });

            modelBuilder.Entity<PermissionDelegate>(entity =>
            {
                entity.HasIndex(e => e.DelegateSid).IsUnique();
            });

            modelBuilder.Entity<ChatRoom>(entity =>
            {
                entity.HasMany(e => e.Members).WithMany();
                entity.Navigation(e => e.Messages).AutoInclude();
                entity.Navigation(e => e.Members).AutoInclude();
            });

            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.Navigation(e => e.User).AutoInclude();
            });

            modelBuilder.Entity<NotificationSubscription>(entity =>
            {
                entity.Navigation(e => e.NotificationTypes).AutoInclude();
            });

            modelBuilder.Entity<UnreadChatMessage>(entity =>
            {
                entity.Navigation(e => e.ChatMessage).AutoInclude();

            });

        }



        public bool EntityIsTracked<TEntry>(TEntry entry)
        {
            if (EqualityComparer<TEntry>.Default.Equals(entry, default)) return false;
            return base.Entry(entry).State != EntityState.Detached;
        }

        public static DatabaseException DownReason { get; set; }




        /// <summary>
        /// This should be private
        /// </summary>
        /// <returns></returns>
        private ServiceConnectionState TestConnection()
        {
            Loggers.DatabaseLogger.Information("Testing Database Connection");
            if (ConnectionString != null)
            {
                //Check for db connection
                try
                {
                    //Handle SQLite
                    if (ConnectionString.FileBased)
                    {
                        Loggers.DatabaseLogger.Information("SQLite configuration detected.");

                        if (ConnectionString.File.Writable)
                        {
                            Loggers.DatabaseLogger.Information("SQLite file/directory is writeablee");

                            if (ConnectionString.File.Exists)
                            {
                                Loggers.DatabaseLogger.Information("SQLite file exists. Returning Status Up.");

                                return ServiceConnectionState.Up;
                            }
                        }
                        else
                        {
                            Loggers.DatabaseLogger.Information("The Sqlite database folder is not writable by the current server user.");

                            DownReason = new("The Sqlite database folder is not writable by the current server user.");
                        }
                        return ServiceConnectionState.Down;
                    }

                    Loggers.DatabaseLogger.Information("Not SQLite, checking database server port.");

                    if (NetworkTools.IsPortOpen(ConnectionString.ServerAddress, ConnectionString.ServerPort))
                    {
                        if (Database.CanConnect())
                        {
                            return ServiceConnectionState.Up;
                        }

                    }
                    else
                    {
                        Loggers.DatabaseLogger.Information("Database server port is not open or reachable.");

                        DownReason = new("The database port is not open or is not reachable.");

                        Database.CloseConnection();
                        // return DatabaseStatus.TablesMissing;
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
                    switch (ex.Number)
                    {
                        case 53:
                            //Server unreachable
                            DownReason = new("The database port is open but connecting as an Sql server failed.");
                            break;


                        case 208:
                            //Tables Missing
                            DownReason = new("The database is missing a table. It may be in a corrupt state.");
                            break;
                        case 18456:
                            //Database may be missing or permission issue
                            DownReason = new("The database server is reachable, but the database could not be found or the" +
                                " credentials provided do not have permission to the database.");
                            break;

                    }

                }


                catch (RetryLimitExceededException)
                {
                    //Couldn't connect to DB
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
                    Loggers.DatabaseLogger.Error(ex.Message + " {@Error}", ex);
                    DownReason = new("The database experienced an unexpected error. " + ex.Message);

                }



            }
            return ServiceConnectionState.Down;

        }



        /// <summary>
        /// Checks if the database seed migration hase been applied
        /// </summary>
        /// <remarks>If the database cannot connect this method returns true</remarks>
        /// <returns>Returns true if the seed migration has been applied, or the database can't be reached, otherwise
        /// returns false.</returns>
        public virtual bool IsSeeded()
        {
            if (AppliedMigrations.Count() > 0) return true;



            try
            {

                if (AuthenticationSettings.FirstOrDefault() == null)
                    return false;
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
                if (!IsSeeded()) return false;
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
                var tableName = Model.FindEntityType(entityType).GetTableName();

                DataTable table = SelectAllDataFromTable(tableName);

                // Create a CSV file name for the table
                var fileName = Path.Combine(directory, tableName + ".csv");
                var file = new SystemFile(fileName);
                file.EnsureCreated();
                // Write the data table to the CSV file
                using (var writer = new StreamWriter(fileName))
                {
                    // Write the column names
                    var columnNames = table.Columns.Cast<DataColumn>().Select(c => c.ColumnName);
                    writer.WriteLine(string.Join(",", columnNames));

                    // Write the rows
                    foreach (DataRow row in table.Rows)
                    {
                        var fields = row.ItemArray.Select(f => f?.ToString());
                        List<string> lines = new();
                        foreach (var field in fields)
                        {
                            lines.Add('"' + field + '"');
                        }
                        writer.WriteLine(string.Join(",", lines));
                    }
                }
            }
        }

        protected virtual DataTable SelectAllDataFromTable(string? tableName)
        {
            throw new NotImplementedException("The SelectAllDataFromTable method has not been implemented");

        }

    }
}
