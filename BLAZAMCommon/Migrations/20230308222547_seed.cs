using System;
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
            migrationBuilder.CreateTable(
                name: "AccessLevels",
                columns: table => new
                {
                    AccessLevelId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessLevels", x => x.AccessLevelId);
                });

            migrationBuilder.CreateTable(
                name: "ActiveDirectoryFields",
                columns: table => new
                {
                    ActiveDirectoryFieldId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FieldName = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveDirectoryFields", x => x.ActiveDirectoryFieldId);
                });

            migrationBuilder.CreateTable(
                name: "ActiveDirectorySettings",
                columns: table => new
                {
                    ADSettingsId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ApplicationBaseDN = table.Column<string>(type: "TEXT", nullable: false),
                    FQDN = table.Column<string>(type: "TEXT", nullable: false),
                    ServerAddress = table.Column<string>(type: "TEXT", nullable: false),
                    ServerPort = table.Column<int>(type: "INTEGER", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    Password = table.Column<string>(type: "TEXT", nullable: false),
                    UseTLS = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveDirectorySettings", x => x.ADSettingsId);
                    table.CheckConstraint("CK_Table_Column", "[ADSettingsId] = 1");
                });

            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    AppSettingsId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LastUpdateCheck = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AppName = table.Column<string>(type: "TEXT", nullable: false),
                    InstallationCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    MOTD = table.Column<string>(type: "TEXT", nullable: true),
                    ForceHTTPS = table.Column<bool>(type: "INTEGER", nullable: false),
                    HttpsPort = table.Column<int>(type: "INTEGER", nullable: true),
                    AppFQDN = table.Column<string>(type: "TEXT", nullable: true),
                    AnalyticsId = table.Column<string>(type: "TEXT", nullable: true),
                    UserHelpdeskURL = table.Column<string>(type: "TEXT", nullable: true),
                    AppIcon = table.Column<byte[]>(type: "BLOB", nullable: true),
                    AutoUpdate = table.Column<bool>(type: "INTEGER", nullable: false),
                    AutoUpdateTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    UpdateBranch = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.AppSettingsId);
                    table.CheckConstraint("CK_Table_Column", "[AppSettingsId] = 1");
                });

            migrationBuilder.CreateTable(
                name: "AuthenticationSettings",
                columns: table => new
                {
                    AuthenticationSettingsId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SessionTimeout = table.Column<int>(type: "INTEGER", nullable: true),
                    AdminPassword = table.Column<string>(type: "TEXT", nullable: true),
                    DuoClientId = table.Column<string>(type: "TEXT", nullable: true),
                    DuoClientSecret = table.Column<string>(type: "TEXT", nullable: true),
                    DuoApiHost = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthenticationSettings", x => x.AuthenticationSettingsId);
                    table.CheckConstraint("CK_Table_Column", "[AuthenticationSettingsId] = 1");
                });

            migrationBuilder.CreateTable(
                name: "ComputerAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    IpAddress = table.Column<string>(type: "TEXT", nullable: true),
                    Action = table.Column<string>(type: "TEXT", nullable: false),
                    Target = table.Column<string>(type: "TEXT", nullable: true),
                    BeforeAction = table.Column<string>(type: "TEXT", nullable: true),
                    AfterAction = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComputerAuditLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DirectoryTemplates",
                columns: table => new
                {
                    DirectoryTemplateId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Category = table.Column<string>(type: "TEXT", nullable: true),
                    ObjectType = table.Column<int>(type: "INTEGER", nullable: false),
                    DisplayNameFormula = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordFormula = table.Column<string>(type: "TEXT", nullable: false),
                    UsernameFormula = table.Column<string>(type: "TEXT", nullable: false),
                    ParentOU = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectoryTemplates", x => x.DirectoryTemplateId);
                });

            migrationBuilder.CreateTable(
                name: "EmailSettings",
                columns: table => new
                {
                    EmailSettingsId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AdminBcc = table.Column<string>(type: "TEXT", nullable: true),
                    FromName = table.Column<string>(type: "TEXT", nullable: true),
                    ReplyToAddress = table.Column<string>(type: "TEXT", nullable: true),
                    ReplyToName = table.Column<string>(type: "TEXT", nullable: true),
                    FromAddress = table.Column<string>(type: "TEXT", nullable: true),
                    UseSMTPAuth = table.Column<bool>(type: "INTEGER", nullable: false),
                    SMTPUsername = table.Column<string>(type: "TEXT", nullable: true),
                    SMTPPassword = table.Column<string>(type: "TEXT", nullable: true),
                    SMTPServer = table.Column<string>(type: "TEXT", nullable: false),
                    SMTPPort = table.Column<int>(type: "INTEGER", nullable: false),
                    UseTLS = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailSettings", x => x.EmailSettingsId);
                    table.CheckConstraint("CK_Table_Column", "[EmailSettingsId] = 1");
                });

            migrationBuilder.CreateTable(
                name: "EmailTemplates",
                columns: table => new
                {
                    EmailTemplateId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TemplateName = table.Column<string>(type: "TEXT", nullable: false),
                    Subject = table.Column<string>(type: "TEXT", nullable: false),
                    Body = table.Column<string>(type: "TEXT", nullable: false),
                    CC = table.Column<string>(type: "TEXT", nullable: false),
                    BCC = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplates", x => x.EmailTemplateId);
                });

            migrationBuilder.CreateTable(
                name: "FieldAccessLevel",
                columns: table => new
                {
                    FieldAccessLevelId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldAccessLevel", x => x.FieldAccessLevelId);
                });

            migrationBuilder.CreateTable(
                name: "GroupAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    IpAddress = table.Column<string>(type: "TEXT", nullable: true),
                    Action = table.Column<string>(type: "TEXT", nullable: false),
                    Target = table.Column<string>(type: "TEXT", nullable: true),
                    BeforeAction = table.Column<string>(type: "TEXT", nullable: true),
                    AfterAction = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupAuditLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LogonAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    IpAddress = table.Column<string>(type: "TEXT", nullable: true),
                    Action = table.Column<string>(type: "TEXT", nullable: false),
                    Target = table.Column<string>(type: "TEXT", nullable: true),
                    BeforeAction = table.Column<string>(type: "TEXT", nullable: true),
                    AfterAction = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogonAuditLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ObjectAccessLevel",
                columns: table => new
                {
                    ObjectAccessLevelId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjectAccessLevel", x => x.ObjectAccessLevelId);
                });

            migrationBuilder.CreateTable(
                name: "ObjectActionFlag",
                columns: table => new
                {
                    ActionAccessFlagId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjectActionFlag", x => x.ActionAccessFlagId);
                });

            migrationBuilder.CreateTable(
                name: "OUAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    IpAddress = table.Column<string>(type: "TEXT", nullable: true),
                    Action = table.Column<string>(type: "TEXT", nullable: false),
                    Target = table.Column<string>(type: "TEXT", nullable: true),
                    BeforeAction = table.Column<string>(type: "TEXT", nullable: true),
                    AfterAction = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OUAuditLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PermissionDelegate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DelegateSid = table.Column<byte[]>(type: "BLOB", nullable: false),
                    IsSuperAdmin = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionDelegate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PermissionMap",
                columns: table => new
                {
                    PermissionMapId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OU = table.Column<string>(type: "TEXT", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionMap", x => x.PermissionMapId);
                });

            migrationBuilder.CreateTable(
                name: "PermissionsAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    IpAddress = table.Column<string>(type: "TEXT", nullable: true),
                    Action = table.Column<string>(type: "TEXT", nullable: false),
                    Target = table.Column<string>(type: "TEXT", nullable: true),
                    BeforeAction = table.Column<string>(type: "TEXT", nullable: true),
                    AfterAction = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionsAuditLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RequestAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    IpAddress = table.Column<string>(type: "TEXT", nullable: true),
                    Action = table.Column<string>(type: "TEXT", nullable: false),
                    Target = table.Column<string>(type: "TEXT", nullable: true),
                    BeforeAction = table.Column<string>(type: "TEXT", nullable: true),
                    AfterAction = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestAuditLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SettingsAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    IpAddress = table.Column<string>(type: "TEXT", nullable: true),
                    Action = table.Column<string>(type: "TEXT", nullable: false),
                    Target = table.Column<string>(type: "TEXT", nullable: true),
                    BeforeAction = table.Column<string>(type: "TEXT", nullable: true),
                    AfterAction = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettingsAuditLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    Action = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemAuditLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    IpAddress = table.Column<string>(type: "TEXT", nullable: true),
                    Action = table.Column<string>(type: "TEXT", nullable: false),
                    Target = table.Column<string>(type: "TEXT", nullable: true),
                    BeforeAction = table.Column<string>(type: "TEXT", nullable: true),
                    AfterAction = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAuditLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    UserSettingsId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserGUID = table.Column<string>(type: "TEXT", nullable: false),
                    APIToken = table.Column<string>(type: "TEXT", nullable: true),
                    Theme = table.Column<string>(type: "TEXT", nullable: true),
                    SearchDisabledUsers = table.Column<bool>(type: "INTEGER", nullable: false),
                    SearchDisabledComputers = table.Column<bool>(type: "INTEGER", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => x.UserSettingsId);
                });

            migrationBuilder.CreateTable(
                name: "DirectoryTemplateFieldValues",
                columns: table => new
                {
                    DirectoryTemplateFieldValueId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FieldActiveDirectoryFieldId = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    DirectoryTemplateId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectoryTemplateFieldValues", x => x.DirectoryTemplateFieldValueId);
                    table.ForeignKey(
                        name: "FK_DirectoryTemplateFieldValues_ActiveDirectoryFields_FieldActiveDirectoryFieldId",
                        column: x => x.FieldActiveDirectoryFieldId,
                        principalTable: "ActiveDirectoryFields",
                        principalColumn: "ActiveDirectoryFieldId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DirectoryTemplateFieldValues_DirectoryTemplates_DirectoryTemplateId",
                        column: x => x.DirectoryTemplateId,
                        principalTable: "DirectoryTemplates",
                        principalColumn: "DirectoryTemplateId");
                });

            migrationBuilder.CreateTable(
                name: "DirectoryTemplateGroups",
                columns: table => new
                {
                    DirectoryTemplateGroupId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GroupSid = table.Column<string>(type: "TEXT", nullable: false),
                    DirectoryTemplateId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectoryTemplateGroups", x => x.DirectoryTemplateGroupId);
                    table.ForeignKey(
                        name: "FK_DirectoryTemplateGroups_DirectoryTemplates_DirectoryTemplateId",
                        column: x => x.DirectoryTemplateId,
                        principalTable: "DirectoryTemplates",
                        principalColumn: "DirectoryTemplateId");
                });

            migrationBuilder.CreateTable(
                name: "AccessLevelFieldMapping",
                columns: table => new
                {
                    FieldAccessMappingId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ActiveDirectoryFieldId = table.Column<int>(type: "INTEGER", nullable: false),
                    FieldAccessLevelId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessLevelFieldMapping", x => x.FieldAccessMappingId);
                    table.ForeignKey(
                        name: "FK_AccessLevelFieldMapping_ActiveDirectoryFields_ActiveDirectoryFieldId",
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
                });

            migrationBuilder.CreateTable(
                name: "AccessLevelObjectMapping",
                columns: table => new
                {
                    ObjectAccessMappingId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ObjectType = table.Column<int>(type: "INTEGER", nullable: false),
                    ObjectAccessLevelId = table.Column<int>(type: "INTEGER", nullable: false),
                    AllowDisabled = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessLevelObjectMapping", x => x.ObjectAccessMappingId);
                    table.ForeignKey(
                        name: "FK_AccessLevelObjectMapping_ObjectAccessLevel_ObjectAccessLevelId",
                        column: x => x.ObjectAccessLevelId,
                        principalTable: "ObjectAccessLevel",
                        principalColumn: "ObjectAccessLevelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActionAccessMapping",
                columns: table => new
                {
                    ActionAccessMappingId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ObjectType = table.Column<int>(type: "INTEGER", nullable: false),
                    AllowOrDeny = table.Column<bool>(type: "INTEGER", nullable: false),
                    ObjectActionActionAccessFlagId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActionAccessMapping", x => x.ActionAccessMappingId);
                    table.ForeignKey(
                        name: "FK_ActionAccessMapping_ObjectActionFlag_ObjectActionActionAccessFlagId",
                        column: x => x.ObjectActionActionAccessFlagId,
                        principalTable: "ObjectActionFlag",
                        principalColumn: "ActionAccessFlagId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccessLevelPermissionMap",
                columns: table => new
                {
                    AccessLevelsAccessLevelId = table.Column<int>(type: "INTEGER", nullable: false),
                    PermissionMapsPermissionMapId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessLevelPermissionMap", x => new { x.AccessLevelsAccessLevelId, x.PermissionMapsPermissionMapId });
                    table.ForeignKey(
                        name: "FK_AccessLevelPermissionMap_AccessLevels_AccessLevelsAccessLevelId",
                        column: x => x.AccessLevelsAccessLevelId,
                        principalTable: "AccessLevels",
                        principalColumn: "AccessLevelId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccessLevelPermissionMap_PermissionMap_PermissionMapsPermissionMapId",
                        column: x => x.PermissionMapsPermissionMapId,
                        principalTable: "PermissionMap",
                        principalColumn: "PermissionMapId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PermissionDelegatePermissionMap",
                columns: table => new
                {
                    PermissionDelegatesId = table.Column<int>(type: "INTEGER", nullable: false),
                    PermissionsMapsPermissionMapId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionDelegatePermissionMap", x => new { x.PermissionDelegatesId, x.PermissionsMapsPermissionMapId });
                    table.ForeignKey(
                        name: "FK_PermissionDelegatePermissionMap_PermissionDelegate_PermissionDelegatesId",
                        column: x => x.PermissionDelegatesId,
                        principalTable: "PermissionDelegate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PermissionDelegatePermissionMap_PermissionMap_PermissionsMapsPermissionMapId",
                        column: x => x.PermissionsMapsPermissionMapId,
                        principalTable: "PermissionMap",
                        principalColumn: "PermissionMapId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccessLevelFieldAccessMapping",
                columns: table => new
                {
                    AccessLevelsAccessLevelId = table.Column<int>(type: "INTEGER", nullable: false),
                    FieldMapFieldAccessMappingId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessLevelFieldAccessMapping", x => new { x.AccessLevelsAccessLevelId, x.FieldMapFieldAccessMappingId });
                    table.ForeignKey(
                        name: "FK_AccessLevelFieldAccessMapping_AccessLevelFieldMapping_FieldMapFieldAccessMappingId",
                        column: x => x.FieldMapFieldAccessMappingId,
                        principalTable: "AccessLevelFieldMapping",
                        principalColumn: "FieldAccessMappingId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccessLevelFieldAccessMapping_AccessLevels_AccessLevelsAccessLevelId",
                        column: x => x.AccessLevelsAccessLevelId,
                        principalTable: "AccessLevels",
                        principalColumn: "AccessLevelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccessLevelObjectAccessMapping",
                columns: table => new
                {
                    AccessLevelsAccessLevelId = table.Column<int>(type: "INTEGER", nullable: false),
                    ObjectMapObjectAccessMappingId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessLevelObjectAccessMapping", x => new { x.AccessLevelsAccessLevelId, x.ObjectMapObjectAccessMappingId });
                    table.ForeignKey(
                        name: "FK_AccessLevelObjectAccessMapping_AccessLevelObjectMapping_ObjectMapObjectAccessMappingId",
                        column: x => x.ObjectMapObjectAccessMappingId,
                        principalTable: "AccessLevelObjectMapping",
                        principalColumn: "ObjectAccessMappingId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccessLevelObjectAccessMapping_AccessLevels_AccessLevelsAccessLevelId",
                        column: x => x.AccessLevelsAccessLevelId,
                        principalTable: "AccessLevels",
                        principalColumn: "AccessLevelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccessLevelActionAccessMapping",
                columns: table => new
                {
                    AccessLevelsAccessLevelId = table.Column<int>(type: "INTEGER", nullable: false),
                    ActionMapActionAccessMappingId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessLevelActionAccessMapping", x => new { x.AccessLevelsAccessLevelId, x.ActionMapActionAccessMappingId });
                    table.ForeignKey(
                        name: "FK_AccessLevelActionAccessMapping_AccessLevels_AccessLevelsAccessLevelId",
                        column: x => x.AccessLevelsAccessLevelId,
                        principalTable: "AccessLevels",
                        principalColumn: "AccessLevelId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccessLevelActionAccessMapping_ActionAccessMapping_ActionMapActionAccessMappingId",
                        column: x => x.ActionMapActionAccessMappingId,
                        principalTable: "ActionAccessMapping",
                        principalColumn: "ActionAccessMappingId",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "IX_PermissionDelegatePermissionMap_PermissionsMapsPermissionMapId",
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
