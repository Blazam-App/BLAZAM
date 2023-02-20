using BLAZAM.Common.Helpers;
using BLAZAM.Common.Models.Database;
using BLAZAM.Common.Models.Database.Audit;
using BLAZAM.Common.Models.Database.Permissions;
using BLAZAM.Common.Models.Database.Templates;
using BLAZAM.Common.Models.Database.User;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace BLAZAM.Common.Data.Database
{
    public class DatabaseContext : DbContext
    {
        private ILoggerFactory loggerFactory;

        /// <summary>
        /// The connection string as set in the ASP Net Core appsettings.json
        /// <para>This should be set before any attempts to connect.</para>
        /// <para>Usually in the Program.Main method before injecting the service.</para>
        /// </summary>
        public static string? ConnectionString { get; set; }
        /// <summary>
        /// The last error message recieved by this instance
        /// </summary>
        public string? LastErrorMessage { get; private set; }

        /// <summary>
        /// Checks the pingabillity and connectivity to the database
        /// </summary>
        public ConnectionStatus Status
        {
            get
            {
                return TestConnection();
            }
        }
        /// <summary>
        /// Pings the configured <see cref="Server"/>
        /// </summary>
        public bool Pingable
        {
            get
            {
                if (Server != null && Server != "")
                    return NetworkTools.PingHost(Server);
                return false;
            }
        }
        /// <summary>
        /// Checks if the configured <see cref="Port"/> on the configured <see cref="Server"/> is open
        /// </summary>
        public int Port
        {
            get
            {
                string connectionString = Database.GetConnectionString();
                if (connectionString != null)
                {
                    int startIndex = connectionString.IndexOf("Data Source=");
                    if (startIndex >= 0)
                    {
                        startIndex += "Data Source=".Length;
                        int endIndex = connectionString.IndexOf(";", startIndex);
                        if (endIndex >= 0)
                        {
                            string dataSourceElement = connectionString.Substring(startIndex, endIndex - startIndex);
                            string[] dataSourceParts = dataSourceElement.Split(':');
                            if (dataSourceParts.Length == 2)
                            {
                                string portFragment = dataSourceParts[1];
                                return int.Parse(portFragment);  // Outputs "serverPort"
                            }
                        }

                    }

                }
                return 1433;
            }
        }
        /// <summary>
        /// The database server as defined in the <see cref="ConnectionString"/>
        /// </summary>
        public string? Server
        {
            get
            {
                try
                {
                    string connectionString = Database.GetConnectionString();
                    if (connectionString != null)
                    {
                        int startIndex = connectionString.IndexOf("Data Source=");
                        if (startIndex >= 0)
                        {
                            startIndex += "Data Source=".Length;
                            int endIndex = connectionString.IndexOf(";", startIndex);
                            if (endIndex >= 0)
                            {
                                string dataSourceElement = connectionString.Substring(startIndex, endIndex - startIndex);
                                string[] dataSourceParts = dataSourceElement.Split(':');
                                if (dataSourceParts.Length > 0)
                                {
                                    string serverFragment = dataSourceParts[0];
                                    return serverFragment;  // Outputs "serverNameOrIp"
                                }
                            }

                        }

                    }
                }
                catch { }
                return null;

            }
        }
        public enum ConnectionStatus
        {
            OK, ServerUnreachable,
            TablesMissing,
            DatabaseConnectionIssue,
            IncompleteConfiguration
        }


        public DatabaseContext(ILoggerFactory loggerFactory, DbContextOptions options) : base(options)
        {
            this.loggerFactory = loggerFactory;
        }

        /*
        public DatabaseContext(string connectionString)
        {
            ConnectionString = connectionString;

            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();

            optionsBuilder.UseSqlServer(connectionString);


        }
        */

        //Permissions
        public DbSet<ActiveDirectoryField> ActiveDirectoryFields { get; set; }
        public DbSet<AccessLevel> AccessLevels { get; set; }
        public DbSet<ObjectAccessMapping> AccessLevelObjectMapping { get; set; }
        public DbSet<FieldAccessMapping> AccessLevelFieldMapping { get; set; }
        public DbSet<FieldAccessLevel> FieldAccessLevel { get; set; }
        public DbSet<ObjectAccessLevel> ObjectAccessLevel { get; set; }
        public DbSet<ActionAccessFlag> ObjectActionFlag { get; set; }

        public DbSet<PrivilegeLevel> PrivilegeLevel { get; set; }
        public DbSet<PrivilegeMap> PrivilegeMap { get; set; }


        //Templates
        public DbSet<DirectoryTemplate> DirectoryTemplates { get; set; }
        public DbSet<DirectoryTemplateFieldValue> DirectoryTemplateFieldValues { get; set; }
        public DbSet<DirectoryTemplateGroup> DirectoryTemplateGroups { get; set; }


        //App Settings
        public DbSet<AppSettings> AppSettings { get; set; }
        public DbSet<ADSettings> ActiveDirectorySettings { get; set; }
        public DbSet<AuthenticationSettings> AuthenticationSettings { get; set; }
        public DbSet<EmailSettings> EmailSettings { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }
        public DbSet<UserSettings> UserSettings { get; set; }



        //Audit Logs
        public DbSet<SystemAuditLog> SystemAuditLog { get; set; }
        public DbSet<LogonAuditLog> LogonAuditLog { get; set; }
        public DbSet<UserAuditLog> UserAuditLog { get; set; }
        public DbSet<GroupAuditLog> GroupAuditLog { get; set; }
        public DbSet<ComputerAuditLog> ComputerAuditLog { get; set; }
        public DbSet<OUAuditLog> OUAuditLog { get; set; }
        public DbSet<RequestAuditLog> RequestAuditLog { get; set; }
        public DbSet<PermissionsAuditLog> PermissionsAuditLog { get; set; }
        public DbSet<SettingsAuditLog> SettingsAuditLog { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(ConnectionString,
                options => options.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(1),
                    errorNumbersToAdd: new List<int>() { }
                    )
                );

            optionsBuilder.UseLoggerFactory(loggerFactory);

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ActiveDirectoryField>().HasData(



                new ActiveDirectoryField { ActiveDirectoryFieldId = 1, FieldName = "sn", DisplayName = "Last Name" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 2, FieldName = "givenname", DisplayName = "First Name" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 3, FieldName = "physicalDeliveryOfficeName", DisplayName = "Office" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 4, FieldName = "employeeId", DisplayName = "Employee ID" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 5, FieldName = "homeDirectory", DisplayName = "Home Directory" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 6, FieldName = "scriptPath", DisplayName = "Logon Script Path" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 7, FieldName = "profilePath", DisplayName = "Profile Path" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 8, FieldName = "homePhone", DisplayName = "Home Phone Number" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 9, FieldName = "streetAddress", DisplayName = "Street Address" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 10, FieldName = "city", DisplayName = "City" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 11, FieldName = "st", DisplayName = "State" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 12, FieldName = "postalCode", DisplayName = "Zip Code" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 13, FieldName = "site", DisplayName = "Site" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 14, FieldName = "name", DisplayName = "Name" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 15, FieldName = "samaccountname", DisplayName = "Username" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 16, FieldName = "objectSID", DisplayName = "SID" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 17, FieldName = "mail", DisplayName = "E-Mail Address" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 18, FieldName = "description", DisplayName = "Description" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 19, FieldName = "displayName", DisplayName = "Display Name" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 20, FieldName = "distinguishedName", DisplayName = "Distinguished Name" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 21, FieldName = "memberOf", DisplayName = "Member Of" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 22, FieldName = "company", DisplayName = "Company" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 23, FieldName = "title", DisplayName = "Title" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 24, FieldName = "userPrincipalName", DisplayName = "User Principal Name" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 25, FieldName = "telephoneNumber", DisplayName = "Telephone Number" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 26, FieldName = "street", DisplayName = "Street" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 27, FieldName = "cn", DisplayName = "Canonical Name" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 28, FieldName = "homeDrive", DisplayName = "Home Drive" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 29, FieldName = "department", DisplayName = "Department" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 30, FieldName = "middleName", DisplayName = "Middle Name" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 31, FieldName = "pager", DisplayName = "Pager" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 32, FieldName = "operatingSystemVersion", DisplayName = "OS" },
                new ActiveDirectoryField { ActiveDirectoryFieldId = 33, FieldName = "accountExpires", DisplayName = "Account Expiration" }

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


            modelBuilder.Entity<PrivilegeMap>(entity =>
            {
                entity.Navigation(e => e.AccessLevels).AutoInclude();

            });


            modelBuilder.Entity<DirectoryTemplate>(entity =>
            {
                entity.Navigation(e => e.AssignedGroupSids).AutoInclude();

            });

            modelBuilder.Entity<AppSettings>(entity =>
            {
                entity.ToTable(t => t.HasCheckConstraint("CK_Table_Column", "[AppSettingsId] = 1"));

            });

            modelBuilder.Entity<ADSettings>(entity =>
            {
                entity.ToTable(t => t.HasCheckConstraint("CK_Table_Column", "[ADSettingsId] = 1"));

            });
            modelBuilder.Entity<AuthenticationSettings>(entity =>
            {
                entity.ToTable(t => t.HasCheckConstraint("CK_Table_Column", "[AuthenticationSettingsId] = 1"));
                entity.HasData(new AuthenticationSettings { AuthenticationSettingsId = 1, AdminPassword = "password" });
            });

            modelBuilder.Entity<EmailSettings>(entity =>
            {
                entity.ToTable(t => t.HasCheckConstraint("CK_Table_Column", "[EmailSettingsId] = 1"));

            });

            modelBuilder.Entity<EmailTemplate>(entity =>
            {
            });

            modelBuilder.Entity<UserSettings>(entity =>
            {
                entity.HasIndex(e => e.UserGUID).IsUnique();
            });

        }












        /// <summary>
        /// This should be private
        /// </summary>
        /// <returns></returns>
        public ConnectionStatus TestConnection()
        {
            if (ConnectionString != null)
            {
                if (Pingable)
                {
                    //Check for db connection
                    try
                    {
                        Database.OpenConnection();


                        //Check for tables
                        if (AllTablesPresent())
                        {
                            //Installation has been completed
                            LastErrorMessage = string.Empty;
                            Database.CloseConnection();

                            return ConnectionStatus.OK;
                        }
                        else
                        {
                            Database.CloseConnection();

                            return ConnectionStatus.TablesMissing;
                        }



                    }
                    catch (SqlException ex)
                    {
                        LastErrorMessage = ex.Message;
                        switch (ex.Number)
                        {
                            case 53:
                                //Server unreachable
                                return ConnectionStatus.ServerUnreachable;

                            case 208:
                                //Tables Missing
                                return ConnectionStatus.TablesMissing;

                            case 18456:
                                //Database may be missing or permission issue

                                return ConnectionStatus.DatabaseConnectionIssue;



                        }

                    }


                    catch (RetryLimitExceededException ex)
                    {
                        //Couldn't connect to DB

                        LastErrorMessage = ex.Message;
                        if (Server != null)
                        {
                            if (NetworkTools.PingHost(Server))
                            {
                                if (NetworkTools.IsPortOpen(Server, Port))
                                {
                                    return ConnectionStatus.TablesMissing;

                                }
                            }
                            else
                            {
                                return ConnectionStatus.ServerUnreachable;
                            }
                        }
                        else
                            return ConnectionStatus.IncompleteConfiguration;
                        return ConnectionStatus.DatabaseConnectionIssue;

                    }
                    catch (Exception ex)
                    {
                        LastErrorMessage = ex.Message;

                        //Installation not completed


                    }
                    LastErrorMessage = "Unknown Error";


                }
                return ConnectionStatus.ServerUnreachable;
            }
            return ConnectionStatus.IncompleteConfiguration;
        }
        public bool AllTablesPresent()
        {

            var type = GetType();
            var properties = type.GetProperties();
            foreach (var property in properties)
            {

                if (property.PropertyType.IsAssignableFrom(typeof(DbSet<>)) && property.GetValue(this) == null)
                {
                    return false;
                }
            }
            if (this.AuthenticationSettings.FirstOrDefault() == null)
                return false;
            return true;
        }



    }
}
