using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Common.Migrations
{
    /// <inheritdoc />
    public partial class moreColumnsForSystemAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AfterAction",
                schema: "Audit",
                table: "SystemAuditLog",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BeforeAction",
                schema: "Audit",
                table: "SystemAuditLog",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                schema: "Audit",
                table: "SystemAuditLog",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Target",
                schema: "Audit",
                table: "SystemAuditLog",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AfterAction",
                schema: "Audit",
                table: "SystemAuditLog");

            migrationBuilder.DropColumn(
                name: "BeforeAction",
                schema: "Audit",
                table: "SystemAuditLog");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                schema: "Audit",
                table: "SystemAuditLog");

            migrationBuilder.DropColumn(
                name: "Target",
                schema: "Audit",
                table: "SystemAuditLog");
        }
    }
}
