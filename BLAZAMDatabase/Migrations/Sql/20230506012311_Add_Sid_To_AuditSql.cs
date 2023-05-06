using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Database.Migrations.Sql
{
    /// <inheritdoc />
    public partial class Add_Sid_To_AuditSql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Sid",
                table: "UserAuditLog",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Sid",
                table: "OUAuditLog",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Sid",
                table: "GroupAuditLog",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Sid",
                table: "ComputerAuditLog",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
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
