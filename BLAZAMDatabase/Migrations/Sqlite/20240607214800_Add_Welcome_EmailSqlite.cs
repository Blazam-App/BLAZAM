using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class Add_Welcome_EmailSqlite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AskForAlternateEmail",
                table: "DirectoryTemplates",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SendWelcomeEmail",
                table: "DirectoryTemplates",
                type: "INTEGER",
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
