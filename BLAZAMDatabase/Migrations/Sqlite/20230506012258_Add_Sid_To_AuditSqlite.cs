using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class Add_Sid_To_AuditSqlite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Sid",
                table: "UserAuditLog",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Sid",
                table: "OUAuditLog",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Sid",
                table: "GroupAuditLog",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Sid",
                table: "ComputerAuditLog",
                type: "TEXT",
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
