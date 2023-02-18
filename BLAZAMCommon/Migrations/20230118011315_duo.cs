using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Common.Migrations
{
    /// <inheritdoc />
    public partial class duo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DuoApiHost",
                table: "AuthenticationSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DuoClientId",
                table: "AuthenticationSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DuoClientSecret",
                table: "AuthenticationSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AuthenticationSettings",
                keyColumn: "AuthenticationSettingsId",
                keyValue: 1,
                columns: new[] { "DuoApiHost", "DuoClientId", "DuoClientSecret" },
                values: new object[] { null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DuoApiHost",
                table: "AuthenticationSettings");

            migrationBuilder.DropColumn(
                name: "DuoClientId",
                table: "AuthenticationSettings");

            migrationBuilder.DropColumn(
                name: "DuoClientSecret",
                table: "AuthenticationSettings");
        }
    }
}
