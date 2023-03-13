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
            migrationBuilder.DropColumn(
                name: "HttpsPort",
                table: "AppSettings");

            migrationBuilder.AddColumn<string>(
                name: "AfterAction",
                table: "SystemAuditLog",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "BeforeAction",
                table: "SystemAuditLog",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "SystemAuditLog",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Target",
                table: "SystemAuditLog",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AfterAction",
                table: "SystemAuditLog");

            migrationBuilder.DropColumn(
                name: "BeforeAction",
                table: "SystemAuditLog");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "SystemAuditLog");

            migrationBuilder.DropColumn(
                name: "Target",
                table: "SystemAuditLog");

            migrationBuilder.AddColumn<int>(
                name: "HttpsPort",
                table: "AppSettings",
                type: "int",
                nullable: true);
        }
    }
}
