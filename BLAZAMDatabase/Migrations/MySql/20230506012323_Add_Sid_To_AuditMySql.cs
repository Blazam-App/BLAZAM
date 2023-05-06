using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Database.Migrations.MySql
{
    /// <inheritdoc />
    public partial class Add_Sid_To_AuditMySql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Sid",
                table: "UserAuditLog",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Sid",
                table: "OUAuditLog",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Sid",
                table: "GroupAuditLog",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Sid",
                table: "ComputerAuditLog",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sid",
                table: "UserAuditLog");

            migrationBuilder.DropColumn(
                name: "Sid",
                table: "OUAuditLog");

            migrationBuilder.DropColumn(
                name: "Sid",
                table: "GroupAuditLog");

            migrationBuilder.DropColumn(
                name: "Sid",
                table: "ComputerAuditLog");
        }
    }
}
