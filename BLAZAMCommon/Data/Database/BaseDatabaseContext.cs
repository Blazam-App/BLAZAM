using BLAZAM.Common.Helpers;
using BLAZAM.Common.Models.Database;
using BLAZAM.Common.Models.Database.Audit;
using BLAZAM.Common.Models.Database.Permissions;
using BLAZAM.Common.Models.Database.Templates;
using BLAZAM.Common.Models.Database.User;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace BLAZAM.Common.Data.Database
{
    public class DatabaseContext : DbContext, IDatabaseContext
    {
        /// <summary>
        /// The connection string as set in the ASP Net Core appsettings.json
        /// <para>This should be set before any attempts to connect.</para>
        /// <para>Usually in the Program.Main method before injecting the service.</para>
        /// </summary>
        public DatabaseConnectionString? ConnectionString { get; set; }

        /// <summary>
        /// Checks the realtime pingabillity and connectivity to the database right now
        /// </summary>
        public virtual DatabaseStatus Status
        {
            get
            {
                return TestConnection();
            }
        }



        static IEnumerable<string> _pendingMigrations;
        public virtual IEnumerable<string> PendingMigrations
        {
            get
            {
                _pendingMigrations ??= Database.GetPendingMigrations();
                return _pendingMigrations;
            }
        }
        static IEnumerable<string> _appliedMigrations;
        private string _dbType;

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

        public DatabaseContext()
        {
        }

        public DatabaseContext(DatabaseConnectionString databaseConnectionString) : base()
        {
            ConnectionString = databaseConnectionString;
            _dbType = "SQL";
        }






        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="options"><inheritdoc/></param>
        public DatabaseContext(DbContextOptions options) : base(options)
        {
        }

        //App Settings
        public virtual DbSet<AppSettings> AppSettings { get; set; }
        public virtual DbSet<ADSettings> ActiveDirectorySettings { get; set; }
        public virtual DbSet<AuthenticationSettings> AuthenticationSettings { get; set; }
        public virtual DbSet<EmailSettings> EmailSettings { get; set; }
        public virtual DbSet<EmailTemplate> EmailTemplates { get; set; }
        public virtual DbSet<UserSettings> UserSettings { get; set; }



        //Audit Logs
        public virtual DbSet<SystemAuditLog> SystemAuditLog { get; set; }
        public virtual DbSet<LogonAuditLog> LogonAuditLog { get; set; }
        public virtual DbSet<UserAuditLog> UserAuditLog { get; set; }
        public virtual DbSet<GroupAuditLog> GroupAuditLog { get; set; }
        public virtual DbSet<ComputerAuditLog> ComputerAuditLog { get; set; }
        public virtual DbSet<OUAuditLog> OUAuditLog { get; set; }
        public virtual DbSet<RequestAuditLog> RequestAuditLog { get; set; }
        public virtual DbSet<PermissionsAuditLog> PermissionsAuditLog { get; set; }
        public virtual DbSet<SettingsAuditLog> SettingsAuditLog { get; set; }



        //Permissions
        public virtual DbSet<ActiveDirectoryField> ActiveDirectoryFields { get; set; }
        public virtual DbSet<AccessLevel> AccessLevels { get; set; }
        public virtual DbSet<ObjectAccessMapping> AccessLevelObjectMapping { get; set; }
        public virtual DbSet<FieldAccessMapping> AccessLevelFieldMapping { get; set; }
        public virtual DbSet<FieldAccessLevel> FieldAccessLevel { get; set; }
        public virtual DbSet<ObjectAccessLevel> ObjectAccessLevel { get; set; }
        public virtual DbSet<ActionAccessFlag> ObjectActionFlag { get; set; }

        public virtual DbSet<PermissionDelegate> PermissionDelegate { get; set; }
        public virtual DbSet<PermissionMap> PermissionMap { get; set; }


        //Templates
        public virtual DbSet<DirectoryTemplate> DirectoryTemplates { get; set; }
        public virtual DbSet<DirectoryTemplateFieldValue> DirectoryTemplateFieldValues { get; set; }
        public virtual DbSet<DirectoryTemplateGroup> DirectoryTemplateGroups { get; set; }



        public static ConfigurationManager Configuration { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //Keeping the contents of this method here for now, delete and replace with NotImplementedException
            //When ready
            //No child classes rely on this
            /*
            _dbType ??= Configuration.GetValue<string>("DatabaseType");
            Console.WriteLine("Database Type: " + _dbType);
            switch (_dbType)
            {

                case "SQL":

                    optionsBuilder.UseSqlServer(
                        Configuration.GetConnectionString("SQLConnectionString"),
                            sqlServerOptionsAction: sqlOptions =>
                            {
                                sqlOptions.EnableRetryOnFailure();

                            }
                                ).EnableSensitiveDataLogging()
                                .LogTo(Loggers.DatabaseLogger.Information);
                    break;
                case "SQLite":

                    optionsBuilder.UseSqlite(
                        Configuration.GetConnectionString("SQLiteConnectionString")).EnableSensitiveDataLogging()
                        .LogTo(Loggers.DatabaseLogger.Information);
                    break;

                case "MySQL":
                    optionsBuilder.UseMySql(ConnectionString?.ConnectionString,
                         serverVersion: new MySqlServerVersion(new Version(8, 0, 32)),
                        mySqlOptionsAction: options =>
                        {
                            options.EnableRetryOnFailure();
                            //options.SetSqlModeOnOpen();

                        })

                        .EnableSensitiveDataLogging()
                                .LogTo(Loggers.DatabaseLogger.Information);
                    break;

            }

            */
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {


            modelBuilder.Entity<ActiveDirectoryField>().HasData(


                new ActiveDirectoryField { ActiveDirectoryFieldId = 1000001, FieldName = "sn", DisplayName = "Last Name" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 1000002, FieldName = "givenname", DisplayName = "First Name" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 1000003, FieldName = "physicalDeliveryOfficeName", DisplayName = "Office" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 1000004, FieldName = "employeeId", DisplayName = "Employee ID" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 1000005, FieldName = "homeDirectory", DisplayName = "Home Directory" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 1000006, FieldName = "scriptPath", DisplayName = "Logon Script Path" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 1000007, FieldName = "profilePath", DisplayName = "Profile Path" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 1000008, FieldName = "homePhone", DisplayName = "Home Phone Number" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 1000009, FieldName = "streetAddress", DisplayName = "Street Address" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 1000010, FieldName = "city", DisplayName = "City" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 1000011, FieldName = "st", DisplayName = "State" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 1000012, FieldName = "postalCode", DisplayName = "Zip Code" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 1000013, FieldName = "site", DisplayName = "Site" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 1000014, FieldName = "name", DisplayName = "Name" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 1000015, FieldName = "samaccountname", DisplayName = "Username" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 1000016, FieldName = "objectSID", DisplayName = "SID" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 1000017, FieldName = "mail", DisplayName = "E-Mail Address" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 1000018, FieldName = "description", DisplayName = "Description" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 1000019, FieldName = "displayName", DisplayName = "Display Name" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 1000020, FieldName = "distinguishedName", DisplayName = "Distinguished Name" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 1000021, FieldName = "memberOf", DisplayName = "Member Of" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 1000022, FieldName = "company", DisplayName = "Company" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 1000023, FieldName = "title", DisplayName = "Title" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 1000024, FieldName = "userPrincipalName", DisplayName = "User Principal Name" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 1000025, FieldName = "telephoneNumber", DisplayName = "Telephone Number" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 1000026, FieldName = "street", DisplayName = "Street" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 1000027, FieldName = "cn", DisplayName = "Canonical Name" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 1000028, FieldName = "homeDrive", DisplayName = "Home Drive" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 1000029, FieldName = "department", DisplayName = "Department" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 1000030, FieldName = "middleName", DisplayName = "Middle Name" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 1000031, FieldName = "pager", DisplayName = "Pager" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 1000032, FieldName = "operatingSystemVersion", DisplayName = "OS" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 1000033, FieldName = "accountExpires", DisplayName = "Account Expiration" }

            );





            modelBuilder.Entity<AccessLevel>(entity =>
            {
                entity.HasData(
                        new AccessLevel { AccessLevelId = 1, Name = "Deny All" }
                );
                entity.Navigation(e => e.ObjectMap).AutoInclude();
                entity.Navigation(e => e.FieldMap).AutoInclude();
                entity.Navigation(e => e.ActionMap).AutoInclude();
            });

            modelBuilder.Entity<FieldAccessLevel>().HasData(
                new FieldAccessLevel() { FieldAccessLevelId = 1, Name = "Deny", Level = 10 },
                new FieldAccessLevel() { FieldAccessLevelId = 2, Name = "Read", Level = 100 },
                new FieldAccessLevel() { FieldAccessLevelId = 3, Name = "Edit", Level = 1000 }
            );
            modelBuilder.Entity<FieldAccessMapping>(entity =>
            {
                entity.Navigation(e => e.Field).AutoInclude();
                entity.Navigation(e => e.FieldAccessLevel).AutoInclude();
            });
            modelBuilder.Entity<ObjectAccessLevel>(entity =>
            {
                entity.HasData(
                new ObjectAccessLevel() { ObjectAccessLevelId = 1, Name = "Deny", Level = 10 },
                new ObjectAccessLevel() { ObjectAccessLevelId = 2, Name = "Read", Level = 1000 });

                entity.Navigation(e => e.ObjectAccessMappings).AutoInclude();
            }

            );
            modelBuilder.Entity<ObjectAccessMapping>(entity =>
            {
                entity.Navigation(e => e.ObjectAccessLevel).AutoInclude();
            });
            modelBuilder.Entity<DirectoryTemplate>(entity =>
            {
                entity.Navigation(e => e.FieldValues).AutoInclude();
            });
            modelBuilder.Entity<DirectoryTemplateFieldValue>(entity =>
            {
                entity.Navigation(e => e.Field).AutoInclude();
            });
            modelBuilder.Entity<ActionAccessFlag>().HasData(
                  new ActionAccessFlag() { ActionAccessFlagId = 1, Name = "Assign" },
                  new ActionAccessFlag() { ActionAccessFlagId = 2, Name = "UnAssign" },
                  new ActionAccessFlag() { ActionAccessFlagId = 3, Name = "Unlock" },
                  new ActionAccessFlag() { ActionAccessFlagId = 4, Name = "Enable" },
                  new ActionAccessFlag() { ActionAccessFlagId = 5, Name = "Disable" },
                  new ActionAccessFlag() { ActionAccessFlagId = 6, Name = "Rename" },
                  new ActionAccessFlag() { ActionAccessFlagId = 7, Name = "Move" },
                  new ActionAccessFlag() { ActionAccessFlagId = 8, Name = "Create" },
                  new ActionAccessFlag() { ActionAccessFlagId = 9, Name = "Delete" }

            );
            modelBuilder.Entity<ActionAccessMapping>(entity =>
            {
                entity.Navigation(e => e.ObjectAction).AutoInclude();
            });


            modelBuilder.Entity<PermissionMap>(entity =>
            {
                entity.Navigation(e => e.AccessLevels).AutoInclude();

            });


            modelBuilder.Entity<DirectoryTemplate>(entity =>
            {
                entity.Navigation(e => e.AssignedGroupSids).AutoInclude();

            });

            modelBuilder.Entity<AppSettings>(entity =>
            {
                entity.Property(e => e.AppSettingsId).ValueGeneratedNever();

                if (Database.IsMySql())
                    entity.ToTable(t => t.HasCheckConstraint("CK_Table_Column", "AppSettingsId = 1"));
                else
                    entity.ToTable(t => t.HasCheckConstraint("CK_Table_Column", "[AppSettingsId] = 1"));
            });

            modelBuilder.Entity<ADSettings>(entity =>
            {
                entity.Property(e => e.ADSettingsId).ValueGeneratedNever();

                if (Database.IsMySql())
                    entity.ToTable(t => t.HasCheckConstraint("CK_Table_Column", "ADSettingsId = 1"));
                else
                    entity.ToTable(t => t.HasCheckConstraint("CK_Table_Column", "[ADSettingsId] = 1"));

            });
            modelBuilder.Entity<AuthenticationSettings>(entity =>
            {
                entity.Property(e => e.AuthenticationSettingsId).ValueGeneratedNever();

                if (Database.IsMySql())
                    entity.ToTable(t => t.HasCheckConstraint("CK_Table_Column", "AuthenticationSettingsId = 1"));

                else
                    entity.ToTable(t => t.HasCheckConstraint("CK_Table_Column", "[AuthenticationSettingsId] = 1"));
                entity.HasData(new AuthenticationSettings
                {
                    AuthenticationSettingsId = 1,
                    AdminPassword = "password"
                });
            });

            modelBuilder.Entity<EmailSettings>(entity =>
            {
                entity.Property(e => e.EmailSettingsId).ValueGeneratedNever();

                if (Database.IsMySql())
                    entity.ToTable(t => t.HasCheckConstraint("CK_Table_Column", "EmailSettingsId = 1"));

                else
                    entity.ToTable(t => t.HasCheckConstraint("CK_Table_Column", "[EmailSettingsId] = 1"));

            });


            modelBuilder.Entity<UserSettings>(entity =>
            {
                entity.HasIndex(e => e.UserGUID).IsUnique();
            });
            modelBuilder.Entity<PermissionDelegate>(entity =>
          {
              entity.HasIndex(e => e.DelegateSid).IsUnique();
          });

        }











        /// <summary>
        /// This should be private
        /// </summary>
        /// <returns></returns>
        private DatabaseStatus TestConnection()
        {
            if (ConnectionString != null)
            {
                //Check for db connection
                try
                {

                    //Handle SQLite
                    if (ConnectionString.FileBased)
                    {
                        if (ConnectionString.File.Writable)
                        {
                            ConnectionString.File.EnsureCreated();
                            return DatabaseStatus.OK;
                        }
                        return DatabaseStatus.DatabaseConnectionIssue;
                    }


                    if (!NetworkTools.IsPortOpen(ConnectionString.ServerAddress, ConnectionString.ServerPort)) return DatabaseStatus.ServerUnreachable;

                    Database.OpenConnection();
                    //Check for tables

                    if (IsSeeded())
                    {
                        //Installation has been completed
                        Database.CloseConnection();
                    }
                    else
                    {
                        Database.CloseConnection();
                       // return DatabaseStatus.TablesMissing;
                    }
                    return DatabaseStatus.OK;

                }
                catch (SqlException ex)
                {
                    switch (ex.Number)
                    {
                        case 53:
                            //Server unreachable
                            return DatabaseStatus.ServerUnreachable;

                        case 208:
                            //Tables Missing
                            return DatabaseStatus.TablesMissing;

                        case 18456:
                            //Database may be missing or permission issue
                            return DatabaseStatus.DatabaseConnectionIssue;
                    }

                }


                catch (RetryLimitExceededException)
                {
                    //Couldn't connect to DB
                    return DatabaseStatus.DatabaseConnectionIssue;

                }
                catch (DatabaseConnectionStringException)
                {
                    return DatabaseStatus.IncompleteConfiguration;
                }
                catch (ApplicationException ex) {
                    
                    return DatabaseStatus.IncompleteConfiguration;
                }
                catch (Exception ex)
                {
                    Loggers.DatabaseLogger.Error(ex.Message, ex);
                    //Installation not completed
                }
                throw new ApplicationException("Unknown error checking connecting to database. The port is open.");



            }
            return DatabaseStatus.IncompleteConfiguration;
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
                if (AppSettings.FirstOrDefault()?.InstallationCompleted == true)
                    return true;
            }
            catch
            {

            }

            var appliedMigs = Database.GetAppliedMigrations();
            //var migs = this.Database.GetPendingMigrations();

            if (appliedMigs.Count() > 0) return true;
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

    }
}
