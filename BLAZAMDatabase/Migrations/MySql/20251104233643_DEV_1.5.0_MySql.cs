using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Database.Migrations.MySql
{
    /// <inheritdoc />
    public partial class DEV_150_MySql : Migration
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
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

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
                name: "GlobalAutomationRuleSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
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
                name: "IX_AutomationRuleExcludedGroupGuid_GlobalAutomationRuleSettings~",
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
                name: "IX_AutomationRuleGroupSids_AutomationRuleActionId",
                table: "AutomationRuleGroupSids",
                column: "AutomationRuleActionId");
        }
    }
}
