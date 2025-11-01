using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class DEV_150_Sqlite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AutomationRuleGroupSids");

            migrationBuilder.AddColumn<int>(
                name: "CustomFieldId",
                table: "NotificationMessages",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FieldId",
                table: "NotificationMessages",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AutomationRuleGroupGuids",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GroupGuid = table.Column<Guid>(type: "TEXT", nullable: false),
                    Assigned = table.Column<bool>(type: "INTEGER", nullable: false),
                    AutomationRuleActionId = table.Column<int>(type: "INTEGER", nullable: false)
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
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalAutomationRuleSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AutomationRuleExcludedGroupGuid",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    GlobalAutomationRuleSettingsId = table.Column<int>(type: "INTEGER", nullable: true)
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
                name: "IX_AutomationRuleExcludedGroupGuid_GlobalAutomationRuleSettingsId",
                table: "AutomationRuleExcludedGroupGuid",
                column: "GlobalAutomationRuleSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRuleGroupGuids_AutomationRuleActionId",
                table: "AutomationRuleGroupGuids",
                column: "AutomationRuleActionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AutomationRuleExcludedGroupGuid");

            migrationBuilder.DropTable(
                name: "AutomationRuleGroupGuids");

            migrationBuilder.DropTable(
                name: "GlobalAutomationRuleSettings");

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

            migrationBuilder.CreateTable(
                name: "AutomationRuleGroupSids",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AutomationRuleActionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Assigned = table.Column<bool>(type: "INTEGER", nullable: false),
                    GroupSid = table.Column<string>(type: "TEXT", nullable: false)
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
