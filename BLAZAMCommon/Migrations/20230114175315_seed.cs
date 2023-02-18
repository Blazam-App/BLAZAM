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
                name: "ActiveDirectoryFields",
                columns: table => new
                {
                    ActiveDirectoryFieldId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FieldName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveDirectoryFields", x => x.ActiveDirectoryFieldId);
                });

            migrationBuilder.CreateTable(
                name: "ActiveDirectorySettings",
                columns: table => new
                {
                    ADSettingsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
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
                    table.PrimaryKey("PK_ActiveDirectorySettings", x => x.ADSettingsId);
                    table.CheckConstraint("CK_Table_Column", "[ADSettingsId] = 1");
                });

            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    AppSettingsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LastUpdateCheck = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AppName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MOTD = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ForceHTTPS = table.Column<bool>(type: "bit", nullable: false),
                    HttpsPort = table.Column<int>(type: "int", nullable: true),
                    AppFQDN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserHelpdeskURL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AppIcon = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.AppSettingsId);
                    table.CheckConstraint("CK_Table_Column1", "[AppSettingsId] = 1");
                });

            migrationBuilder.CreateTable(
                name: "AuditLog",
                columns: table => new
                {
                    AuditLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BeforeAction = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AfterAction = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLog", x => x.AuditLogId);
                });

            migrationBuilder.CreateTable(
                name: "AuthenticationSettings",
                columns: table => new
                {
                    AuthenticationSettingsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionTimeout = table.Column<int>(type: "int", nullable: true),
                    AdminPassword = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthenticationSettings", x => x.AuthenticationSettingsId);
                    table.CheckConstraint("CK_Table_Column2", "[AuthenticationSettingsId] = 1");
                });

            migrationBuilder.CreateTable(
                name: "EmailSettings",
                columns: table => new
                {
                    EmailSettingsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
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
                    table.PrimaryKey("PK_EmailSettings", x => x.EmailSettingsId);
                    table.CheckConstraint("CK_Table_Column3", "[EmailSettingsId] = 1");
                });

            migrationBuilder.CreateTable(
                name: "EmailTemplates",
                columns: table => new
                {
                    EmailTemplateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BCC = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplates", x => x.EmailTemplateId);
                });

            migrationBuilder.CreateTable(
                name: "FieldAccessLevel",
                columns: table => new
                {
                    FieldAccessLevelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldAccessLevel", x => x.FieldAccessLevelId);
                });

            migrationBuilder.CreateTable(
                name: "ObjectAccessLevel",
                columns: table => new
                {
                    ObjectAccessLevelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjectAccessLevel", x => x.ObjectAccessLevelId);
                });

            migrationBuilder.CreateTable(
                name: "ObjectActionFlag",
                columns: table => new
                {
                    ActionAccessFlagId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjectActionFlag", x => x.ActionAccessFlagId);
                });

            migrationBuilder.CreateTable(
                name: "PrivilegeLevel",
                columns: table => new
                {
                    PrivilegeLevelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupSID = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    IsSuperAdmin = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivilegeLevel", x => x.PrivilegeLevelId);
                });

            migrationBuilder.CreateTable(
                name: "PrivilegeMap",
                columns: table => new
                {
                    PrivilegeMapId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OU = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivilegeMap", x => x.PrivilegeMapId);
                });

            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    UserSettingsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserGUID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    APIToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Theme = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => x.UserSettingsId);
                });

            migrationBuilder.CreateTable(
                name: "AccessLevelFieldMapping",
                columns: table => new
                {
                    FieldAccessMappingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActiveDirectoryFieldId = table.Column<int>(type: "int", nullable: false),
                    FieldAccessLevelId = table.Column<int>(type: "int", nullable: false)
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
                    ObjectAccessMappingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ObjectType = table.Column<int>(type: "int", nullable: false),
                    ObjectAccessLevelId = table.Column<int>(type: "int", nullable: false),
                    AllowDisabled = table.Column<bool>(type: "bit", nullable: false)
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
                    ActionAccessMappingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ObjectType = table.Column<int>(type: "int", nullable: false),
                    AllowOrDeny = table.Column<bool>(type: "bit", nullable: false),
                    ObjectActionActionAccessFlagId = table.Column<int>(type: "int", nullable: false)
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
                name: "AccessLevels",
                columns: table => new
                {
                    AccessLevelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrivilegeMapId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessLevels", x => x.AccessLevelId);
                    table.ForeignKey(
                        name: "FK_AccessLevels_PrivilegeMap_PrivilegeMapId",
                        column: x => x.PrivilegeMapId,
                        principalTable: "PrivilegeMap",
                        principalColumn: "PrivilegeMapId");
                });

            migrationBuilder.CreateTable(
                name: "PrivilegeLevelPrivilegeMap",
                columns: table => new
                {
                    PrivilegeLevelsPrivilegeLevelId = table.Column<int>(type: "int", nullable: false),
                    PrivilegeMapsPrivilegeMapId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivilegeLevelPrivilegeMap", x => new { x.PrivilegeLevelsPrivilegeLevelId, x.PrivilegeMapsPrivilegeMapId });
                    table.ForeignKey(
                        name: "FK_PrivilegeLevelPrivilegeMap_PrivilegeLevel_PrivilegeLevelsPrivilegeLevelId",
                        column: x => x.PrivilegeLevelsPrivilegeLevelId,
                        principalTable: "PrivilegeLevel",
                        principalColumn: "PrivilegeLevelId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PrivilegeLevelPrivilegeMap_PrivilegeMap_PrivilegeMapsPrivilegeMapId",
                        column: x => x.PrivilegeMapsPrivilegeMapId,
                        principalTable: "PrivilegeMap",
                        principalColumn: "PrivilegeMapId",
                        onDelete: ReferentialAction.Cascade);
                });

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
                    AccessLevelsAccessLevelId = table.Column<int>(type: "int", nullable: false),
                    ObjectMapObjectAccessMappingId = table.Column<int>(type: "int", nullable: false)
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

            migrationBuilder.InsertData(
                table: "AccessLevels",
                columns: new[] { "AccessLevelId", "Name", "PrivilegeMapId" },
                values: new object[] { 1, "Deny All", null });

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
                    { 27, "Container Name", "cn" },
                    { 28, "Home Drive", "homeDrive" },
                    { 29, "Department", "department" },
                    { 30, "Middle Name", "middleName" },
                    { 31, "Pager", "pager" }
                });

            migrationBuilder.InsertData(
                table: "AuthenticationSettings",
                columns: new[] { "AuthenticationSettingsId", "AdminPassword", "SessionTimeout" },
                values: new object[] { 1, "password", 900000 });

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
                name: "IX_AccessLevels_PrivilegeMapId",
                table: "AccessLevels",
                column: "PrivilegeMapId");

            migrationBuilder.CreateIndex(
                name: "IX_ActionAccessMapping_ObjectActionActionAccessFlagId",
                table: "ActionAccessMapping",
                column: "ObjectActionActionAccessFlagId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivilegeLevelPrivilegeMap_PrivilegeMapsPrivilegeMapId",
                table: "PrivilegeLevelPrivilegeMap",
                column: "PrivilegeMapsPrivilegeMapId");

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
                name: "ActiveDirectorySettings");

            migrationBuilder.DropTable(
                name: "AppSettings");

            migrationBuilder.DropTable(
                name: "AuditLog");

            migrationBuilder.DropTable(
                name: "AuthenticationSettings");

            migrationBuilder.DropTable(
                name: "EmailSettings");

            migrationBuilder.DropTable(
                name: "EmailTemplates");

            migrationBuilder.DropTable(
                name: "PrivilegeLevelPrivilegeMap");

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
                name: "PrivilegeLevel");

            migrationBuilder.DropTable(
                name: "ObjectActionFlag");

            migrationBuilder.DropTable(
                name: "ActiveDirectoryFields");

            migrationBuilder.DropTable(
                name: "FieldAccessLevel");

            migrationBuilder.DropTable(
                name: "ObjectAccessLevel");

            migrationBuilder.DropTable(
                name: "PrivilegeMap");
        }
    }
}
