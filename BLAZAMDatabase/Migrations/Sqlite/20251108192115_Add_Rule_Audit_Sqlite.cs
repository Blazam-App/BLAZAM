using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class Add_Rule_Audit_Sqlite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RulesEnabled",
                table: "GlobalAutomationRuleSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "AutomationRuleAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EventType = table.Column<int>(type: "INTEGER", nullable: false),
                    AutomationRuleId = table.Column<int>(type: "INTEGER", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetGuid = table.Column<string>(type: "TEXT", nullable: true),
                    Message = table.Column<string>(type: "TEXT", nullable: true),
                    StackTrace = table.Column<string>(type: "TEXT", nullable: true),
                    RuleSnapshot = table.Column<string>(type: "TEXT", nullable: true),
                    ActionSnapshot = table.Column<string>(type: "TEXT", nullable: true),
                    FilterSnapshot = table.Column<string>(type: "TEXT", nullable: true),
                    MatchesFilter = table.Column<bool>(type: "INTEGER", nullable: true),
                    Trigger = table.Column<int>(type: "INTEGER", nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRuleAuditLog_AutomationRuleId",
                table: "AutomationRuleAuditLog",
                column: "AutomationRuleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AutomationRuleAuditLog");

            migrationBuilder.DropColumn(
                name: "RulesEnabled",
                table: "GlobalAutomationRuleSettings");
        }
    }
}
