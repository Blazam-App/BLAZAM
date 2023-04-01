using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BLAZAM.Common.Migrations.Sql
{
    /// <inheritdoc />
    public partial class Seedv3Sql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccessLevels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessLevels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ActiveDirectoryFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FieldName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveDirectoryFields", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ActiveDirectorySettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    ApplicationBaseDN = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FQDN = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ServerAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ServerPort = table.Column<int>(type: "int", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UseTLS = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveDirectorySettings", x => x.Id);
                    table.CheckConstraint("CK_Table_Column", "[Id] = 1");
                });

            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    LastUpdateCheck = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AppName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InstallationCompleted = table.Column<bool>(type: "bit", nullable: false),
                    MOTD = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ForceHTTPS = table.Column<bool>(type: "bit", nullable: false),
                    AppFQDN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AnalyticsId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserHelpdeskURL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AppIcon = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    AutoUpdate = table.Column<bool>(type: "bit", nullable: false),
                    AutoUpdateTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    UpdateBranch = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Id);
                    table.CheckConstraint("CK_Table_Column1", "[Id] = 1");
                });

            migrationBuilder.CreateTable(
                name: "AuthenticationSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    SessionTimeout = table.Column<int>(type: "int", nullable: true),
                    AdminPassword = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DuoClientId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DuoClientSecret = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DuoApiHost = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthenticationSettings", x => x.Id);
                    table.CheckConstraint("CK_Table_Column2", "[Id] = 1");
                });

            migrationBuilder.CreateTable(
                name: "ComputerAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Target = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BeforeAction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AfterAction = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComputerAuditLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DirectoryTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ObjectType = table.Column<int>(type: "int", nullable: false),
                    DisplayNameFormula = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordFormula = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UsernameFormula = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParentOU = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectoryTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    AdminBcc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FromName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReplyToAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReplyToName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FromAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UseSMTPAuth = table.Column<bool>(type: "bit", nullable: false),
                    SMTPUsername = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SMTPPassword = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SMTPServer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SMTPPort = table.Column<int>(type: "int", nullable: false),
                    UseTLS = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailSettings", x => x.Id);
                    table.CheckConstraint("CK_Table_Column3", "[Id] = 1");
                });

            migrationBuilder.CreateTable(
                name: "EmailTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BCC = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FieldAccessLevel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldAccessLevel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GroupAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Target = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BeforeAction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AfterAction = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupAuditLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LogonAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Target = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BeforeAction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AfterAction = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogonAuditLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ObjectAccessLevel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjectAccessLevel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ObjectActionFlag",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjectActionFlag", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OUAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Target = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BeforeAction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AfterAction = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OUAuditLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PermissionDelegate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DelegateSid = table.Column<byte[]>(type: "varbinary(900)", nullable: false),
                    IsSuperAdmin = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionDelegate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PermissionMap",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OU = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionMap", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PermissionsAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Target = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BeforeAction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AfterAction = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionsAuditLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RequestAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Target = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BeforeAction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AfterAction = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestAuditLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SettingsAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Target = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BeforeAction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AfterAction = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettingsAuditLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Target = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BeforeAction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AfterAction = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemAuditLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Target = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BeforeAction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AfterAction = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAuditLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserGUID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    APIToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Theme = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SearchDisabledUsers = table.Column<bool>(type: "bit", nullable: false),
                    SearchDisabledComputers = table.Column<bool>(type: "bit", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProfilePicture = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DirectoryTemplateFieldValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FieldId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DirectoryTemplateId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectoryTemplateFieldValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DirectoryTemplateFieldValues_ActiveDirectoryFields_FieldId",
                        column: x => x.FieldId,
                        principalTable: "ActiveDirectoryFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DirectoryTemplateFieldValues_DirectoryTemplates_DirectoryTemplateId",
                        column: x => x.DirectoryTemplateId,
                        principalTable: "DirectoryTemplates",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DirectoryTemplateGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupSid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DirectoryTemplateId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectoryTemplateGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DirectoryTemplateGroups_DirectoryTemplates_DirectoryTemplateId",
                        column: x => x.DirectoryTemplateId,
                        principalTable: "DirectoryTemplates",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AccessLevelFieldMapping",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActiveDirectoryFieldId = table.Column<int>(type: "int", nullable: false),
                    FieldAccessLevelId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessLevelFieldMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccessLevelFieldMapping_ActiveDirectoryFields_ActiveDirectoryFieldId",
                        column: x => x.ActiveDirectoryFieldId,
                        principalTable: "ActiveDirectoryFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccessLevelFieldMapping_FieldAccessLevel_FieldAccessLevelId",
                        column: x => x.FieldAccessLevelId,
                        principalTable: "FieldAccessLevel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccessLevelObjectMapping",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ObjectType = table.Column<int>(type: "int", nullable: false),
                    ObjectAccessLevelId = table.Column<int>(type: "int", nullable: false),
                    AllowDisabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessLevelObjectMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccessLevelObjectMapping_ObjectAccessLevel_ObjectAccessLevelId",
                        column: x => x.ObjectAccessLevelId,
                        principalTable: "ObjectAccessLevel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActionAccessMapping",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ObjectType = table.Column<int>(type: "int", nullable: false),
                    AllowOrDeny = table.Column<bool>(type: "bit", nullable: false),
                    ObjectActionId = table.Column<int>(type: "int", nullable: false),
                    AccessLevelId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActionAccessMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActionAccessMapping_AccessLevels_AccessLevelId",
                        column: x => x.AccessLevelId,
                        principalTable: "AccessLevels",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ActionAccessMapping_ObjectActionFlag_ObjectActionId",
                        column: x => x.ObjectActionId,
                        principalTable: "ObjectActionFlag",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccessLevelPermissionMapping",
                columns: table => new
                {
                    AccessLevelsId = table.Column<int>(type: "int", nullable: false),
                    PermissionMapsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessLevelPermissionMapping", x => new { x.AccessLevelsId, x.PermissionMapsId });
                    table.ForeignKey(
                        name: "FK_AccessLevelPermissionMapping_AccessLevels_AccessLevelsId",
                        column: x => x.AccessLevelsId,
                        principalTable: "AccessLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccessLevelPermissionMapping_PermissionMap_PermissionMapsId",
                        column: x => x.PermissionMapsId,
                        principalTable: "PermissionMap",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PermissionDelegatePermissionMapping",
                columns: table => new
                {
                    PermissionDelegatesId = table.Column<int>(type: "int", nullable: false),
                    PermissionsMapsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionDelegatePermissionMapping", x => new { x.PermissionDelegatesId, x.PermissionsMapsId });
                    table.ForeignKey(
                        name: "FK_PermissionDelegatePermissionMapping_PermissionDelegate_PermissionDelegatesId",
                        column: x => x.PermissionDelegatesId,
                        principalTable: "PermissionDelegate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PermissionDelegatePermissionMapping_PermissionMap_PermissionsMapsId",
                        column: x => x.PermissionsMapsId,
                        principalTable: "PermissionMap",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserNotifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Link = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Expires = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Dismissable = table.Column<bool>(type: "bit", nullable: false),
                    AppUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserNotifications_UserSettings_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "UserSettings",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AccessLevelFieldAccessMapping",
                columns: table => new
                {
                    AccessLevelsId = table.Column<int>(type: "int", nullable: false),
                    FieldMapId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessLevelFieldAccessMapping", x => new { x.AccessLevelsId, x.FieldMapId });
                    table.ForeignKey(
                        name: "FK_AccessLevelFieldAccessMapping_AccessLevelFieldMapping_FieldMapId",
                        column: x => x.FieldMapId,
                        principalTable: "AccessLevelFieldMapping",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccessLevelFieldAccessMapping_AccessLevels_AccessLevelsId",
                        column: x => x.AccessLevelsId,
                        principalTable: "AccessLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccessLevelObjectAccessMapping",
                columns: table => new
                {
                    AccessLevelsId = table.Column<int>(type: "int", nullable: false),
                    ObjectMapId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessLevelObjectAccessMapping", x => new { x.AccessLevelsId, x.ObjectMapId });
                    table.ForeignKey(
                        name: "FK_AccessLevelObjectAccessMapping_AccessLevelObjectMapping_ObjectMapId",
                        column: x => x.ObjectMapId,
                        principalTable: "AccessLevelObjectMapping",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccessLevelObjectAccessMapping_AccessLevels_AccessLevelsId",
                        column: x => x.AccessLevelsId,
                        principalTable: "AccessLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AccessLevels",
                columns: new[] { "Id", "DeletedAt", "Name" },
                values: new object[] { 1, null, "Deny All" });

            migrationBuilder.InsertData(
                table: "ActiveDirectoryFields",
                columns: new[] { "Id", "DisplayName", "FieldName" },
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
                    { 26, "PO Box", "postOfficeBox" },
                    { 27, "Canonical Name", "cn" },
                    { 28, "Home Drive", "homeDrive" },
                    { 29, "Department", "department" },
                    { 30, "Middle Name", "middleName" },
                    { 31, "Pager", "pager" },
                    { 32, "OS", "operatingSystemVersion" },
                    { 33, "Account Expiration", "accountExpires" },
                    { 34, "Manager", "manager" }
                });

            migrationBuilder.InsertData(
                table: "AuthenticationSettings",
                columns: new[] { "Id", "AdminPassword", "DuoApiHost", "DuoClientId", "DuoClientSecret", "SessionTimeout" },
                values: new object[] { 1, "password", null, null, null, null });

            migrationBuilder.InsertData(
                table: "FieldAccessLevel",
                columns: new[] { "Id", "Level", "Name" },
                values: new object[,]
                {
                    { 1, 10, "Deny" },
                    { 2, 100, "Read" },
                    { 3, 1000, "Edit" }
                });

            migrationBuilder.InsertData(
                table: "ObjectAccessLevel",
                columns: new[] { "Id", "Level", "Name" },
                values: new object[,]
                {
                    { 1, 10, "Deny" },
                    { 2, 1000, "Read" }
                });

            migrationBuilder.InsertData(
                table: "ObjectActionFlag",
                columns: new[] { "Id", "Name" },
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
                name: "IX_AccessLevelFieldAccessMapping_FieldMapId",
                table: "AccessLevelFieldAccessMapping",
                column: "FieldMapId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessLevelFieldMapping_ActiveDirectoryFieldId",
                table: "AccessLevelFieldMapping",
                column: "ActiveDirectoryFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessLevelFieldMapping_FieldAccessLevelId",
                table: "AccessLevelFieldMapping",
                column: "FieldAccessLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessLevelObjectAccessMapping_ObjectMapId",
                table: "AccessLevelObjectAccessMapping",
                column: "ObjectMapId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessLevelObjectMapping_ObjectAccessLevelId",
                table: "AccessLevelObjectMapping",
                column: "ObjectAccessLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessLevelPermissionMapping_PermissionMapsId",
                table: "AccessLevelPermissionMapping",
                column: "PermissionMapsId");

            migrationBuilder.CreateIndex(
                name: "IX_ActionAccessMapping_AccessLevelId",
                table: "ActionAccessMapping",
                column: "AccessLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_ActionAccessMapping_ObjectActionId",
                table: "ActionAccessMapping",
                column: "ObjectActionId");

            migrationBuilder.CreateIndex(
                name: "IX_DirectoryTemplateFieldValues_DirectoryTemplateId",
                table: "DirectoryTemplateFieldValues",
                column: "DirectoryTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_DirectoryTemplateFieldValues_FieldId",
                table: "DirectoryTemplateFieldValues",
                column: "FieldId");

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
                name: "IX_PermissionDelegatePermissionMapping_PermissionsMapsId",
                table: "PermissionDelegatePermissionMapping",
                column: "PermissionsMapsId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_AppUserId",
                table: "UserNotifications",
                column: "AppUserId");

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
                name: "AccessLevelFieldAccessMapping");

            migrationBuilder.DropTable(
                name: "AccessLevelObjectAccessMapping");

            migrationBuilder.DropTable(
                name: "AccessLevelPermissionMapping");

            migrationBuilder.DropTable(
                name: "ActionAccessMapping");

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
                name: "PermissionDelegatePermissionMapping");

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
                name: "UserNotifications");

            migrationBuilder.DropTable(
                name: "AccessLevelFieldMapping");

            migrationBuilder.DropTable(
                name: "AccessLevelObjectMapping");

            migrationBuilder.DropTable(
                name: "AccessLevels");

            migrationBuilder.DropTable(
                name: "ObjectActionFlag");

            migrationBuilder.DropTable(
                name: "DirectoryTemplates");

            migrationBuilder.DropTable(
                name: "PermissionDelegate");

            migrationBuilder.DropTable(
                name: "PermissionMap");

            migrationBuilder.DropTable(
                name: "UserSettings");

            migrationBuilder.DropTable(
                name: "ActiveDirectoryFields");

            migrationBuilder.DropTable(
                name: "FieldAccessLevel");

            migrationBuilder.DropTable(
                name: "ObjectAccessLevel");
        }
    }
}
