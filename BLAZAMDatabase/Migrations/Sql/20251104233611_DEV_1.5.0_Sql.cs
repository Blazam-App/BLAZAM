using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Database.Migrations.Sql
{
    /// <inheritdoc />
    public partial class DEV_150_Sql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AutomationRuleGroupSids");

            migrationBuilder.RenameColumn(
                name: "AllowAccessRequest",
                table: "GlobalPermissionSettings",
                newName: "AllowFieldAccessRequest");

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
                type: "bit",
                nullable: false,
                defaultValue: false);

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
                        .Annotation("SqlServer:Identity", "1, 1")
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
                name: "AutomationRuleExcludedGroupGuid");

            migrationBuilder.DropTable(
                name: "AutomationRuleGroupGuids");

            migrationBuilder.DropTable(
                name: "GlobalPermissionRequestFields");

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
                name: "IX_AutomationRuleGroupSids_AutomationRuleActionId",
                table: "AutomationRuleGroupSids",
                column: "AutomationRuleActionId");
        }
    }
}
