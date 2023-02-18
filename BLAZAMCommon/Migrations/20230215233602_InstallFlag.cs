using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Common.Migrations
{
    /// <inheritdoc />
    public partial class InstallFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MatchedUsernameResolution",
                table: "DirectoryTemplates");

            migrationBuilder.DropColumn(
                name: "SwapUsernamesOnDisabledMatch",
                table: "DirectoryTemplates");

            migrationBuilder.AddColumn<bool>(
                name: "InstallationCompleted",
                table: "AppSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InstallationCompleted",
                table: "AppSettings");

            migrationBuilder.AddColumn<int>(
                name: "MatchedUsernameResolution",
                table: "DirectoryTemplates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "SwapUsernamesOnDisabledMatch",
                table: "DirectoryTemplates",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
