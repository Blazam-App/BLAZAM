using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BLAZAM.Common.Migrations
{
    /// <inheritdoc />
    public partial class seed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AccessLevels",
                columns: table => new
                {
                    AccessLevelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessLevels", x => x.AccessLevelId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ActiveDirectoryFields",
                columns: table => new
                {
                    ActiveDirectoryFieldId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FieldName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DisplayName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveDirectoryFields", x => x.ActiveDirectoryFieldId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ActiveDirectorySettings",
                columns: table => new
                {
                    ADSettingsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ApplicationBaseDN = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FQDN = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ServerAddress = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ServerPort = table.Column<int>(type: "int", nullable: false),
                    Username = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Password = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UseTLS = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveDirectorySettings", x => x.ADSettingsId);
                    table.CheckConstraint("CK_Table_Column", "[ADSettingsId] = 1");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    AppSettingsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LastUpdateCheck = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    AppName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InstallationCompleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MOTD = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ForceHTTPS = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    HttpsPort = table.Column<int>(type: "int", nullable: true),
                    AppFQDN = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AnalyticsId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserHelpdeskURL = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AppIcon = table.Column<byte[]>(type: "longblob", nullable: true),
                    AutoUpdate = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AutoUpdateTime = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    UpdateBranch = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.AppSettingsId);
                    table.CheckConstraint("CK_Table_Column1", "[AppSettingsId] = 1");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AuthenticationSettings",
                columns: table => new
                {
                    AuthenticationSettingsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SessionTimeout = table.Column<int>(type: "int", nullable: true),
                    AdminPassword = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DuoClientId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DuoClientSecret = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DuoApiHost = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthenticationSettings", x => x.AuthenticationSettingsId);
                    table.CheckConstraint("CK_Table_Column2", "[AuthenticationSettingsId] = 1");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ComputerAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Timestamp = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Username = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpAddress = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Action = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Target = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BeforeAction = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AfterAction = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComputerAuditLog", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DirectoryTemplates",
                columns: table => new
                {
                    DirectoryTemplateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Category = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ObjectType = table.Column<int>(type: "int", nullable: false),
                    DisplayNameFormula = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PasswordFormula = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UsernameFormula = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParentOU = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectoryTemplates", x => x.DirectoryTemplateId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EmailSettings",
                columns: table => new
                {
                    EmailSettingsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Enabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AdminBcc = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FromName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReplyToAddress = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReplyToName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FromAddress = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UseSMTPAuth = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SMTPUsername = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SMTPPassword = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SMTPServer = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SMTPPort = table.Column<int>(type: "int", nullable: false),
                    UseTLS = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailSettings", x => x.EmailSettingsId);
                    table.CheckConstraint("CK_Table_Column3", "[EmailSettingsId] = 1");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EmailTemplates",
                columns: table => new
                {
                    EmailTemplateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TemplateName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Subject = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Body = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CC = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BCC = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplates", x => x.EmailTemplateId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FieldAccessLevel",
                columns: table => new
                {
                    FieldAccessLevelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Level = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldAccessLevel", x => x.FieldAccessLevelId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GroupAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Timestamp = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Username = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpAddress = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Action = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Target = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BeforeAction = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AfterAction = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupAuditLog", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LogonAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Timestamp = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Username = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpAddress = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Action = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Target = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BeforeAction = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AfterAction = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogonAuditLog", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ObjectAccessLevel",
                columns: table => new
                {
                    ObjectAccessLevelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Level = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjectAccessLevel", x => x.ObjectAccessLevelId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ObjectActionFlag",
                columns: table => new
                {
                    ActionAccessFlagId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjectActionFlag", x => x.ActionAccessFlagId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OUAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Timestamp = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Username = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpAddress = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Action = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Target = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BeforeAction = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AfterAction = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OUAuditLog", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PermissionDelegate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DelegateSid = table.Column<byte[]>(type: "varbinary(3072)", nullable: false),
                    IsSuperAdmin = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionDelegate", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PermissionMap",
                columns: table => new
                {
                    PermissionMapId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OU = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionMap", x => x.PermissionMapId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PermissionsAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Timestamp = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Username = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpAddress = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Action = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Target = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BeforeAction = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AfterAction = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionsAuditLog", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RequestAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Timestamp = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Username = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpAddress = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Action = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Target = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BeforeAction = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AfterAction = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestAuditLog", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SettingsAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Timestamp = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Username = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpAddress = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Action = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Target = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BeforeAction = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AfterAction = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettingsAuditLog", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SystemAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Timestamp = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Username = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Action = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemAuditLog", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Timestamp = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Username = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpAddress = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Action = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Target = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BeforeAction = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AfterAction = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAuditLog", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    UserSettingsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserGUID = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    APIToken = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Theme = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SearchDisabledUsers = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SearchDisabledComputers = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Username = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => x.UserSettingsId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DirectoryTemplateFieldValues",
                columns: table => new
                {
                    DirectoryTemplateFieldValueId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FieldActiveDirectoryFieldId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DirectoryTemplateId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectoryTemplateFieldValues", x => x.DirectoryTemplateFieldValueId);
                    table.ForeignKey(
                        name: "FK_DirectoryTemplateFieldValues_ActiveDirectoryFields_FieldActi~",
                        column: x => x.FieldActiveDirectoryFieldId,
                        principalTable: "ActiveDirectoryFields",
                        principalColumn: "ActiveDirectoryFieldId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DirectoryTemplateFieldValues_DirectoryTemplates_DirectoryTem~",
                        column: x => x.DirectoryTemplateId,
                        principalTable: "DirectoryTemplates",
                        principalColumn: "DirectoryTemplateId");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DirectoryTemplateGroups",
                columns: table => new
                {
                    DirectoryTemplateGroupId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GroupSid = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DirectoryTemplateId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectoryTemplateGroups", x => x.DirectoryTemplateGroupId);
                    table.ForeignKey(
                        name: "FK_DirectoryTemplateGroups_DirectoryTemplates_DirectoryTemplate~",
                        column: x => x.DirectoryTemplateId,
                        principalTable: "DirectoryTemplates",
                        principalColumn: "DirectoryTemplateId");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AccessLevelFieldMapping",
                columns: table => new
                {
                    FieldAccessMappingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ActiveDirectoryFieldId = table.Column<int>(type: "int", nullable: false),
                    FieldAccessLevelId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessLevelFieldMapping", x => x.FieldAccessMappingId);
                    table.ForeignKey(
                        name: "FK_AccessLevelFieldMapping_ActiveDirectoryFields_ActiveDirector~",
                        column: x => x.ActiveDirectoryFieldId,
                        principalTable: "ActiveDirectoryFields",
                        principalColumn: "ActiveDirectoryFieldId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccessLevelFieldMapping_FieldAccessLevel_FieldAccessLevelId",
                        column: x => x.FieldAccessLevelId,
                        principalTable: "FieldAccessLevel",
                        principalColumn: "FieldAccessLevelId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AccessLevelObjectMapping",
                columns: table => new
                {
                    ObjectAccessMappingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ObjectType = table.Column<int>(type: "int", nullable: false),
                    ObjectAccessLevelId = table.Column<int>(type: "int", nullable: false),
                    AllowDisabled = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessLevelObjectMapping", x => x.ObjectAccessMappingId);
                    table.ForeignKey(
                        name: "FK_AccessLevelObjectMapping_ObjectAccessLevel_ObjectAccessLevel~",
                        column: x => x.ObjectAccessLevelId,
                        principalTable: "ObjectAccessLevel",
                        principalColumn: "ObjectAccessLevelId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ActionAccessMapping",
                columns: table => new
                {
                    ActionAccessMappingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ObjectType = table.Column<int>(type: "int", nullable: false),
                    AllowOrDeny = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ObjectActionActionAccessFlagId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActionAccessMapping", x => x.ActionAccessMappingId);
                    table.ForeignKey(
                        name: "FK_ActionAccessMapping_ObjectActionFlag_ObjectActionActionAcces~",
                        column: x => x.ObjectActionActionAccessFlagId,
                        principalTable: "ObjectActionFlag",
                        principalColumn: "ActionAccessFlagId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AccessLevelPermissionMap",
                columns: table => new
                {
                    AccessLevelsAccessLevelId = table.Column<int>(type: "int", nullable: false),
                    PermissionMapsPermissionMapId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessLevelPermissionMap", x => new { x.AccessLevelsAccessLevelId, x.PermissionMapsPermissionMapId });
                    table.ForeignKey(
                        name: "FK_AccessLevelPermissionMap_AccessLevels_AccessLevelsAccessLeve~",
                        column: x => x.AccessLevelsAccessLevelId,
                        principalTable: "AccessLevels",
                        principalColumn: "AccessLevelId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccessLevelPermissionMap_PermissionMap_PermissionMapsPermiss~",
                        column: x => x.PermissionMapsPermissionMapId,
                        principalTable: "PermissionMap",
                        principalColumn: "PermissionMapId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PermissionDelegatePermissionMap",
                columns: table => new
                {
                    PermissionDelegatesId = table.Column<int>(type: "int", nullable: false),
                    PermissionsMapsPermissionMapId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionDelegatePermissionMap", x => new { x.PermissionDelegatesId, x.PermissionsMapsPermissionMapId });
                    table.ForeignKey(
                        name: "FK_PermissionDelegatePermissionMap_PermissionDelegate_Permissio~",
                        column: x => x.PermissionDelegatesId,
                        principalTable: "PermissionDelegate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PermissionDelegatePermissionMap_PermissionMap_PermissionsMap~",
                        column: x => x.PermissionsMapsPermissionMapId,
                        principalTable: "PermissionMap",
                        principalColumn: "PermissionMapId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AccessLevelFieldAccessMapping",
                columns: table => new
                {
                    AccessLevelsAccessLevelId = table.Column<int>(type: "int", nullable: false),
                    FieldMapFieldAccessMappingId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessLevelFieldAccessMapping", x => new { x.AccessLevelsAccessLevelId, x.FieldMapFieldAccessMappingId });
                    table.ForeignKey(
                        name: "FK_AccessLevelFieldAccessMapping_AccessLevelFieldMapping_FieldM~",
                        column: x => x.FieldMapFieldAccessMappingId,
                        principalTable: "AccessLevelFieldMapping",
                        principalColumn: "FieldAccessMappingId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccessLevelFieldAccessMapping_AccessLevels_AccessLevelsAcces~",
                        column: x => x.AccessLevelsAccessLevelId,
                        principalTable: "AccessLevels",
                        principalColumn: "AccessLevelId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AccessLevelObjectAccessMapping",
                columns: table => new
                {
                    AccessLevelsAccessLevelId = table.Column<int>(type: "int", nullable: false),
                    ObjectMapObjectAccessMappingId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessLevelObjectAccessMapping", x => new { x.AccessLevelsAccessLevelId, x.ObjectMapObjectAccessMappingId });
                    table.ForeignKey(
                        name: "FK_AccessLevelObjectAccessMapping_AccessLevelObjectMapping_Obje~",
                        column: x => x.ObjectMapObjectAccessMappingId,
                        principalTable: "AccessLevelObjectMapping",
                        principalColumn: "ObjectAccessMappingId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccessLevelObjectAccessMapping_AccessLevels_AccessLevelsAcce~",
                        column: x => x.AccessLevelsAccessLevelId,
                        principalTable: "AccessLevels",
                        principalColumn: "AccessLevelId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AccessLevelActionAccessMapping",
                columns: table => new
                {
                    AccessLevelsAccessLevelId = table.Column<int>(type: "int", nullable: false),
                    ActionMapActionAccessMappingId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessLevelActionAccessMapping", x => new { x.AccessLevelsAccessLevelId, x.ActionMapActionAccessMappingId });
                    table.ForeignKey(
                        name: "FK_AccessLevelActionAccessMapping_AccessLevels_AccessLevelsAcce~",
                        column: x => x.AccessLevelsAccessLevelId,
                        principalTable: "AccessLevels",
                        principalColumn: "AccessLevelId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccessLevelActionAccessMapping_ActionAccessMapping_ActionMap~",
                        column: x => x.ActionMapActionAccessMappingId,
                        principalTable: "ActionAccessMapping",
                        principalColumn: "ActionAccessMappingId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "AccessLevels",
                columns: new[] { "AccessLevelId", "Name" },
                values: new object[] { 1, "Deny All" });

            migrationBuilder.InsertData(
                table: "ActiveDirectoryFields",
                columns: new[] { "ActiveDirectoryFieldId", "DisplayName", "FieldName" },
                values: new object[,]
                {
                    { 1, "Last Name", "sn" },
                    { 2, "First Name", "givenname" },
                    { 3, "Office", "physicalDeliveryOfficeName" },
                    { 4, "Employee ID", "employeeId" },
                    { 5, "Home Directory", "homeDirectory" },
                    { 6, "Logon Script Path", "scriptPath" },
                    { 7, "Profile Path", "profilePath" },
                    { 8, "Home Phone Number", "homePhone" },
                    { 9, "Street Address", "streetAddress" },
                    { 10, "City", "city" },
                    { 11, "State", "st" },
                    { 12, "Zip Code", "postalCode" },
                    { 13, "Site", "site" },
                    { 14, "Name", "name" },
                    { 15, "Username", "samaccountname" },
                    { 16, "SID", "objectSID" },
                    { 17, "E-Mail Address", "mail" },
                    { 18, "Description", "description" },
                    { 19, "Display Name", "displayName" },
                    { 20, "Distinguished Name", "distinguishedName" },
                    { 21, "Member Of", "memberOf" },
                    { 22, "Company", "company" },
                    { 23, "Title", "title" },
                    { 24, "User Principal Name", "userPrincipalName" },
                    { 25, "Telephone Number", "telephoneNumber" },
                    { 26, "Street", "street" },
                    { 27, "Canonical Name", "cn" },
                    { 28, "Home Drive", "homeDrive" },
                    { 29, "Department", "department" },
                    { 30, "Middle Name", "middleName" },
                    { 31, "Pager", "pager" },
                    { 32, "OS", "operatingSystemVersion" },
                    { 33, "Account Expiration", "accountExpires" }
                });

            migrationBuilder.InsertData(
                table: "AuthenticationSettings",
                columns: new[] { "AuthenticationSettingsId", "AdminPassword", "DuoApiHost", "DuoClientId", "DuoClientSecret", "SessionTimeout" },
                values: new object[] { 1, "password", null, null, null, 900000 });

            migrationBuilder.InsertData(
                table: "FieldAccessLevel",
                columns: new[] { "FieldAccessLevelId", "Level", "Name" },
                values: new object[,]
                {
                    { 1, 10, "Deny" },
                    { 2, 100, "Read" },
                    { 3, 1000, "Edit" }
                });

            migrationBuilder.InsertData(
                table: "ObjectAccessLevel",
                columns: new[] { "ObjectAccessLevelId", "Level", "Name" },
                values: new object[,]
                {
                    { 1, 10, "Deny" },
                    { 2, 1000, "Read" }
                });

            migrationBuilder.InsertData(
                table: "ObjectActionFlag",
                columns: new[] { "ActionAccessFlagId", "Name" },
                values: new object[,]
                {
                    { 1, "Assign" },
                    { 2, "UnAssign" },
                    { 3, "Unlock" },
                    { 4, "Enable" },
                    { 5, "Disable" },
                    { 6, "Rename" },
                    { 7, "Move" },
                    { 8, "Create" },
                    { 9, "Delete" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccessLevelActionAccessMapping_ActionMapActionAccessMappingId",
                table: "AccessLevelActionAccessMapping",
                column: "ActionMapActionAccessMappingId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessLevelFieldAccessMapping_FieldMapFieldAccessMappingId",
                table: "AccessLevelFieldAccessMapping",
                column: "FieldMapFieldAccessMappingId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessLevelFieldMapping_ActiveDirectoryFieldId",
                table: "AccessLevelFieldMapping",
                column: "ActiveDirectoryFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessLevelFieldMapping_FieldAccessLevelId",
                table: "AccessLevelFieldMapping",
                column: "FieldAccessLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessLevelObjectAccessMapping_ObjectMapObjectAccessMappingId",
                table: "AccessLevelObjectAccessMapping",
                column: "ObjectMapObjectAccessMappingId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessLevelObjectMapping_ObjectAccessLevelId",
                table: "AccessLevelObjectMapping",
                column: "ObjectAccessLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessLevelPermissionMap_PermissionMapsPermissionMapId",
                table: "AccessLevelPermissionMap",
                column: "PermissionMapsPermissionMapId");

            migrationBuilder.CreateIndex(
                name: "IX_ActionAccessMapping_ObjectActionActionAccessFlagId",
                table: "ActionAccessMapping",
                column: "ObjectActionActionAccessFlagId");

            migrationBuilder.CreateIndex(
                name: "IX_DirectoryTemplateFieldValues_DirectoryTemplateId",
                table: "DirectoryTemplateFieldValues",
                column: "DirectoryTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_DirectoryTemplateFieldValues_FieldActiveDirectoryFieldId",
                table: "DirectoryTemplateFieldValues",
                column: "FieldActiveDirectoryFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_DirectoryTemplateGroups_DirectoryTemplateId",
                table: "DirectoryTemplateGroups",
                column: "DirectoryTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_DirectoryTemplates_Name",
                table: "DirectoryTemplates",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PermissionDelegate_DelegateSid",
                table: "PermissionDelegate",
                column: "DelegateSid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PermissionDelegatePermissionMap_PermissionsMapsPermissionMap~",
                table: "PermissionDelegatePermissionMap",
                column: "PermissionsMapsPermissionMapId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSettings_UserGUID",
                table: "UserSettings",
                column: "UserGUID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessLevelActionAccessMapping");

            migrationBuilder.DropTable(
                name: "AccessLevelFieldAccessMapping");

            migrationBuilder.DropTable(
                name: "AccessLevelObjectAccessMapping");

            migrationBuilder.DropTable(
                name: "AccessLevelPermissionMap");

            migrationBuilder.DropTable(
                name: "ActiveDirectorySettings");

            migrationBuilder.DropTable(
                name: "AppSettings");

            migrationBuilder.DropTable(
                name: "AuthenticationSettings");

            migrationBuilder.DropTable(
                name: "ComputerAuditLog");

            migrationBuilder.DropTable(
                name: "DirectoryTemplateFieldValues");

            migrationBuilder.DropTable(
                name: "DirectoryTemplateGroups");

            migrationBuilder.DropTable(
                name: "EmailSettings");

            migrationBuilder.DropTable(
                name: "EmailTemplates");

            migrationBuilder.DropTable(
                name: "GroupAuditLog");

            migrationBuilder.DropTable(
                name: "LogonAuditLog");

            migrationBuilder.DropTable(
                name: "OUAuditLog");

            migrationBuilder.DropTable(
                name: "PermissionDelegatePermissionMap");

            migrationBuilder.DropTable(
                name: "PermissionsAuditLog");

            migrationBuilder.DropTable(
                name: "RequestAuditLog");

            migrationBuilder.DropTable(
                name: "SettingsAuditLog");

            migrationBuilder.DropTable(
                name: "SystemAuditLog");

            migrationBuilder.DropTable(
                name: "UserAuditLog");

            migrationBuilder.DropTable(
                name: "UserSettings");

            migrationBuilder.DropTable(
                name: "ActionAccessMapping");

            migrationBuilder.DropTable(
                name: "AccessLevelFieldMapping");

            migrationBuilder.DropTable(
                name: "AccessLevelObjectMapping");

            migrationBuilder.DropTable(
                name: "AccessLevels");

            migrationBuilder.DropTable(
                name: "DirectoryTemplates");

            migrationBuilder.DropTable(
                name: "PermissionDelegate");

            migrationBuilder.DropTable(
                name: "PermissionMap");

            migrationBuilder.DropTable(
                name: "ObjectActionFlag");

            migrationBuilder.DropTable(
                name: "ActiveDirectoryFields");

            migrationBuilder.DropTable(
                name: "FieldAccessLevel");

            migrationBuilder.DropTable(
                name: "ObjectAccessLevel");
        }
    }
}
