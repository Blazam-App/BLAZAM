using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Database.Migrations.Sql
{
    /// <inheritdoc />
    public partial class DEV_ExcludedGroups_Sql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "AutomationRuleExcludedGroupSid",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Sid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GlobalAutomationRuleSettingsId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationRuleExcludedGroupSid", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutomationRuleExcludedGroupSid_GlobalAutomationRuleSettings_GlobalAutomationRuleSettingsId",
                        column: x => x.GlobalAutomationRuleSettingsId,
                        principalTable: "GlobalAutomationRuleSettings",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRuleExcludedGroupSid_GlobalAutomationRuleSettingsId",
                table: "AutomationRuleExcludedGroupSid",
                column: "GlobalAutomationRuleSettingsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AutomationRuleExcludedGroupSid");

            migrationBuilder.DropTable(
                name: "GlobalAutomationRuleSettings");
        }
    }
}
