using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Database.Migrations.Sql
{
    /// <inheritdoc />
    public partial class Add_Rule_Audit_Sql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RulesEnabled",
                table: "GlobalAutomationRuleSettings",
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
                    Trigger = table.Column<int>(type: "int", nullable: true)
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
