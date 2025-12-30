using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Database.Migrations.Sql
{
    /// <inheritdoc />
    public partial class _150_Update_Sql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Create the new join table first
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
                        name: "FK_DirectoryTemplateDirectoryTemplateGroup_DirectoryTemplateGroups_AssignedGroupSidsId",
                        column: x => x.AssignedGroupSidsId,
                        principalTable: "DirectoryTemplateGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DirectoryTemplateDirectoryTemplateGroup_DirectoryTemplates_TemplatesId",
                        column: x => x.TemplatesId,
                        principalTable: "DirectoryTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DirectoryTemplateDirectoryTemplateGroup_TemplatesId",
                table: "DirectoryTemplateDirectoryTemplateGroup",
                column: "TemplatesId");

            // 2. Copy existing relationships into the join table
            migrationBuilder.Sql(@"
INSERT INTO [DirectoryTemplateDirectoryTemplateGroup] ([AssignedGroupSidsId], [TemplatesId])
SELECT [Id], [DirectoryTemplateId]
FROM [DirectoryTemplateGroups]
WHERE [DirectoryTemplateId] IS NOT NULL;
");

            // 3. Now drop the old column and constraints
            migrationBuilder.DropForeignKey(
                name: "FK_DirectoryTemplateGroups_DirectoryTemplates_DirectoryTemplateId",
                table: "DirectoryTemplateGroups");

            migrationBuilder.DropIndex(
                name: "IX_DirectoryTemplateGroups_DirectoryTemplateId",
                table: "DirectoryTemplateGroups");

            migrationBuilder.DropColumn(
                name: "DirectoryTemplateId",
                table: "DirectoryTemplateGroups");

            // Standard migration operations continue...
            migrationBuilder.DropTable(
                name: "AutomationRuleGroupSids");

            migrationBuilder.RenameColumn(
                name: "AllowAccessRequest",
                table: "GlobalPermissionSettings",
                newName: "AllowActionAccessRequest");

            migrationBuilder.AddColumn<string>(
                name: "JsonSettings",
                table: "UserDashboardWidgets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllowPasswordReset",
                table: "PermissionDelegate",
                type: "bit",
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
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequirePINOnPasswordReset",
                table: "PermissionDelegate",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequireQAOnPasswordReset",
                table: "PermissionDelegate",
                type: "bit",
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
                name: "AllowFieldAccessRequest",
                table: "GlobalPermissionSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "AutomationRuleAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    AutomationRuleId = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    TargetGuid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StackTrace = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RuleSnapshot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActionSnapshot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FilterSnapshot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MatchesFilter = table.Column<bool>(type: "bit", nullable: true),
                    Trigger = table.Column<int>(type: "int", nullable: true),
                    ExecutionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "AutomationRuleGroupGuids",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Assigned = table.Column<bool>(type: "bit", nullable: false),
                    AutomationRuleActionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationRuleGroupGuids", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutomationRuleGroupGuids_AutomationRuleActions_AutomationRuleActionId",
                        column: x => x.AutomationRuleActionId,
                        principalTable: "AutomationRuleActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GlobalAutomationRuleSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RulesEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalAutomationRuleSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GlobalPermissionRequestFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AllowEdit = table.Column<bool>(type: "bit", nullable: false),
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
                        name: "FK_GlobalPermissionRequestFields_CustomActiveDirectoryFields_CustomFieldId",
                        column: x => x.CustomFieldId,
                        principalTable: "CustomActiveDirectoryFields",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserPasswordResetSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PIN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Question1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Answer1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Question2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Answer2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Question3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Answer3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResetToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TokenExpiration = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "AutomationRuleExcludedGroupGuid",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GlobalAutomationRuleSettingsId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationRuleExcludedGroupGuid", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutomationRuleExcludedGroupGuid_GlobalAutomationRuleSettings_GlobalAutomationRuleSettingsId",
                        column: x => x.GlobalAutomationRuleSettingsId,
                        principalTable: "GlobalAutomationRuleSettings",
                        principalColumn: "Id");
                });

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
                name: "IX_AutomationRuleExcludedGroupGuid_GlobalAutomationRuleSettingsId",
                table: "AutomationRuleExcludedGroupGuid",
                column: "GlobalAutomationRuleSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRuleGroupGuids_AutomationRuleActionId",
                table: "AutomationRuleGroupGuids",
                column: "AutomationRuleActionId");

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
                name: "FK_NotificationMessages_CustomActiveDirectoryFields_CustomFieldId",
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
                name: "FK_NotificationMessages_CustomActiveDirectoryFields_CustomFieldId",
                table: "NotificationMessages");

            migrationBuilder.DropTable(
                name: "AutomationRuleAuditLog");

            migrationBuilder.DropTable(
                name: "AutomationRuleExcludedGroupGuid");

            migrationBuilder.DropTable(
                name: "AutomationRuleGroupGuids");

            // Moved DropTable for DirectoryTemplateDirectoryTemplateGroup to end of method

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
                name: "AllowFieldAccessRequest",
                table: "GlobalPermissionSettings");

            migrationBuilder.RenameColumn(
                name: "AllowActionAccessRequest",
                table: "GlobalPermissionSettings",
                newName: "AllowAccessRequest");

            // Restore the old column
            migrationBuilder.AddColumn<int>(
                name: "DirectoryTemplateId",
                table: "DirectoryTemplateGroups",
                type: "int",
                nullable: true);

            // Copy data back from join table
            migrationBuilder.Sql(@"
UPDATE g
SET g.[DirectoryTemplateId] = m.[TemplateId]
FROM [DirectoryTemplateGroups] g
JOIN (
    SELECT [AssignedGroupSidsId] AS [GroupId], MIN([TemplatesId]) AS [TemplateId]
    FROM [DirectoryTemplateDirectoryTemplateGroup]
    GROUP BY [AssignedGroupSidsId]
) m ON g.[Id] = m.[GroupId];
");

            // Now drop the join table
            migrationBuilder.DropTable(
                name: "DirectoryTemplateDirectoryTemplateGroup");

            migrationBuilder.CreateTable(
                name: "AutomationRuleGroupSids",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AutomationRuleActionId = table.Column<int>(type: "int", nullable: false),
                    Assigned = table.Column<bool>(type: "bit", nullable: false),
                    GroupSid = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationRuleGroupSids", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutomationRuleGroupSids_AutomationRuleActions_AutomationRuleActionId",
                        column: x => x.AutomationRuleActionId,
                        principalTable: "AutomationRuleActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "FK_DirectoryTemplateGroups_DirectoryTemplates_DirectoryTemplateId",
                table: "DirectoryTemplateGroups",
                column: "DirectoryTemplateId",
                principalTable: "DirectoryTemplates",
                principalColumn: "Id");
        }
    }
}
