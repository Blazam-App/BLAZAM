using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Database.Migrations.MySql
{
    /// <inheritdoc />
    public partial class Add_Welcome_EmailMySql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AskForAlternateEmail",
                table: "DirectoryTemplates",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SendWelcomeEmail",
                table: "DirectoryTemplates",
                type: "tinyint(1)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AskForAlternateEmail",
                table: "DirectoryTemplates");

            migrationBuilder.DropColumn(
                name: "SendWelcomeEmail",
                table: "DirectoryTemplates");
        }
    }
}
