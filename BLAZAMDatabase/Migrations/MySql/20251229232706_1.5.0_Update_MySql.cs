using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Database.Migrations.MySql
{
    /// <inheritdoc />
    public partial class _150_Update_MySql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DirectoryTemplateGroups_DirectoryTemplates_DirectoryTemplate~",
                table: "DirectoryTemplateGroups");

            migrationBuilder.DropTable(
                name: "AutomationRuleGroupSids");

            migrationBuilder.DropIndex(
                name: "IX_DirectoryTemplateGroups_DirectoryTemplateId",
                table: "DirectoryTemplateGroups");

            migrationBuilder.DropColumn(
                name: "DirectoryTemplateId",
                table: "DirectoryTemplateGroups");

            migrationBuilder.RenameColumn(
                name: "AllowAccessRequest",
                table: "GlobalPermissionSettings",
                newName: "AllowFieldAccessRequest");

            migrationBuilder.AddColumn<string>(
                name: "JsonSettings",
                table: "UserDashboardWidgets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "AllowPasswordReset",
                table: "PermissionDelegate",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MinimumPINLength",
                table: "PermissionDelegate",
                type: "int",
                nullable: false,
                defaultValue: 4);

            migrationBuilder.AddColumn<bool>(
                name: "RequireEmailOnPasswordReset",
                table: "PermissionDelegate",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequirePINOnPasswordReset",
                table: "PermissionDelegate",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequireQAOnPasswordReset",
                table: "PermissionDelegate",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CustomFieldId",
                table: "NotificationMessages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FieldId",
                table: "NotificationMessages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllowActionAccessRequest",
                table: "GlobalPermissionSettings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "AutomationRuleAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Timestamp = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    AutomationRuleId = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    TargetGuid = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Message = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StackTrace = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RuleSnapshot = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ActionSnapshot = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FilterSnapshot = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MatchesFilter = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    Trigger = table.Column<int>(type: "int", nullable: true),
                    ExecutionId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationRuleAuditLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutomationRuleAuditLog_AutomationRules_AutomationRuleId",
                        column: x => x.AutomationRuleId,
                        principalTable: "AutomationRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AutomationRuleGroupGuids",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GroupGuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Assigned = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AutomationRuleActionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationRuleGroupGuids", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutomationRuleGroupGuids_AutomationRuleActions_AutomationRul~",
                        column: x => x.AutomationRuleActionId,
                        principalTable: "AutomationRuleActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DirectoryTemplateDirectoryTemplateGroup",
                columns: table => new
                {
                    AssignedGroupSidsId = table.Column<int>(type: "int", nullable: false),
                    TemplatesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectoryTemplateDirectoryTemplateGroup", x => new { x.AssignedGroupSidsId, x.TemplatesId });
                    table.ForeignKey(
                        name: "FK_DirectoryTemplateDirectoryTemplateGroup_DirectoryTemplateGro~",
                        column: x => x.AssignedGroupSidsId,
                        principalTable: "DirectoryTemplateGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DirectoryTemplateDirectoryTemplateGroup_DirectoryTemplates_T~",
                        column: x => x.TemplatesId,
                        principalTable: "DirectoryTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GlobalAutomationRuleSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RulesEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalAutomationRuleSettings", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GlobalPermissionRequestFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AllowEdit = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    FieldId = table.Column<int>(type: "int", nullable: true),
                    CustomFieldId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalPermissionRequestFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GlobalPermissionRequestFields_ActiveDirectoryFields_FieldId",
                        column: x => x.FieldId,
                        principalTable: "ActiveDirectoryFields",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GlobalPermissionRequestFields_CustomActiveDirectoryFields_Cu~",
                        column: x => x.CustomFieldId,
                        principalTable: "CustomActiveDirectoryFields",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserPasswordResetSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PIN = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Question1 = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Answer1 = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Question2 = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Answer2 = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Question3 = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Answer3 = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ResetToken = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TokenExpiration = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPasswordResetSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPasswordResetSettings_UserSettings_UserId",
                        column: x => x.UserId,
                        principalTable: "UserSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AutomationRuleExcludedGroupGuid",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Guid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    GlobalAutomationRuleSettingsId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationRuleExcludedGroupGuid", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutomationRuleExcludedGroupGuid_GlobalAutomationRuleSettings~",
                        column: x => x.GlobalAutomationRuleSettingsId,
                        principalTable: "GlobalAutomationRuleSettings",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 41,
                column: "FieldType",
                value: 5);

            migrationBuilder.InsertData(
                table: "ActiveDirectoryFields",
                columns: new[] { "Id", "DisplayName", "FieldName", "FieldType", "PropertyName" },
                values: new object[] { 45, "LAPS Password", "msLAPS-Password", 0, "LapsPassword" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationMessages_CustomFieldId",
                table: "NotificationMessages",
                column: "CustomFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationMessages_FieldId",
                table: "NotificationMessages",
                column: "FieldId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRuleAuditLog_AutomationRuleId",
                table: "AutomationRuleAuditLog",
                column: "AutomationRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRuleExcludedGroupGuid_GlobalAutomationRuleSettings~",
                table: "AutomationRuleExcludedGroupGuid",
                column: "GlobalAutomationRuleSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRuleGroupGuids_AutomationRuleActionId",
                table: "AutomationRuleGroupGuids",
                column: "AutomationRuleActionId");

            migrationBuilder.CreateIndex(
                name: "IX_DirectoryTemplateDirectoryTemplateGroup_TemplatesId",
                table: "DirectoryTemplateDirectoryTemplateGroup",
                column: "TemplatesId");

            migrationBuilder.CreateIndex(
                name: "IX_GlobalPermissionRequestFields_CustomFieldId",
                table: "GlobalPermissionRequestFields",
                column: "CustomFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_GlobalPermissionRequestFields_FieldId",
                table: "GlobalPermissionRequestFields",
                column: "FieldId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPasswordResetSettings_UserId",
                table: "UserPasswordResetSettings",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationMessages_ActiveDirectoryFields_FieldId",
                table: "NotificationMessages",
                column: "FieldId",
                principalTable: "ActiveDirectoryFields",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationMessages_CustomActiveDirectoryFields_CustomField~",
                table: "NotificationMessages",
                column: "CustomFieldId",
                principalTable: "CustomActiveDirectoryFields",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationMessages_ActiveDirectoryFields_FieldId",
                table: "NotificationMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_NotificationMessages_CustomActiveDirectoryFields_CustomField~",
                table: "NotificationMessages");

            migrationBuilder.DropTable(
                name: "AutomationRuleAuditLog");

            migrationBuilder.DropTable(
                name: "AutomationRuleExcludedGroupGuid");

            migrationBuilder.DropTable(
                name: "AutomationRuleGroupGuids");

            migrationBuilder.DropTable(
                name: "DirectoryTemplateDirectoryTemplateGroup");

            migrationBuilder.DropTable(
                name: "GlobalPermissionRequestFields");

            migrationBuilder.DropTable(
                name: "UserPasswordResetSettings");

            migrationBuilder.DropTable(
                name: "GlobalAutomationRuleSettings");

            migrationBuilder.DropIndex(
                name: "IX_NotificationMessages_CustomFieldId",
                table: "NotificationMessages");

            migrationBuilder.DropIndex(
                name: "IX_NotificationMessages_FieldId",
                table: "NotificationMessages");

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DropColumn(
                name: "JsonSettings",
                table: "UserDashboardWidgets");

            migrationBuilder.DropColumn(
                name: "AllowPasswordReset",
                table: "PermissionDelegate");

            migrationBuilder.DropColumn(
                name: "MinimumPINLength",
                table: "PermissionDelegate");

            migrationBuilder.DropColumn(
                name: "RequireEmailOnPasswordReset",
                table: "PermissionDelegate");

            migrationBuilder.DropColumn(
                name: "RequirePINOnPasswordReset",
                table: "PermissionDelegate");

            migrationBuilder.DropColumn(
                name: "RequireQAOnPasswordReset",
                table: "PermissionDelegate");

            migrationBuilder.DropColumn(
                name: "CustomFieldId",
                table: "NotificationMessages");

            migrationBuilder.DropColumn(
                name: "FieldId",
                table: "NotificationMessages");

            migrationBuilder.DropColumn(
                name: "AllowActionAccessRequest",
                table: "GlobalPermissionSettings");

            migrationBuilder.RenameColumn(
                name: "AllowFieldAccessRequest",
                table: "GlobalPermissionSettings",
                newName: "AllowAccessRequest");

            migrationBuilder.AddColumn<int>(
                name: "DirectoryTemplateId",
                table: "DirectoryTemplateGroups",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AutomationRuleGroupSids",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AutomationRuleActionId = table.Column<int>(type: "int", nullable: false),
                    Assigned = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    GroupSid = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationRuleGroupSids", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutomationRuleGroupSids_AutomationRuleActions_AutomationRule~",
                        column: x => x.AutomationRuleActionId,
                        principalTable: "AutomationRuleActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 41,
                column: "FieldType",
                value: 6);

            migrationBuilder.CreateIndex(
                name: "IX_DirectoryTemplateGroups_DirectoryTemplateId",
                table: "DirectoryTemplateGroups",
                column: "DirectoryTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRuleGroupSids_AutomationRuleActionId",
                table: "AutomationRuleGroupSids",
                column: "AutomationRuleActionId");

            migrationBuilder.AddForeignKey(
                name: "FK_DirectoryTemplateGroups_DirectoryTemplates_DirectoryTemplate~",
                table: "DirectoryTemplateGroups",
                column: "DirectoryTemplateId",
                principalTable: "DirectoryTemplates",
                principalColumn: "Id");
        }
    }
}
