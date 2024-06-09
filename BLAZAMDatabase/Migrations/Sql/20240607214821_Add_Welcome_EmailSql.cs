using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Database.Migrations.Sql
{
    /// <inheritdoc />
    public partial class Add_Welcome_EmailSql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AskForAlternateEmail",
                table: "DirectoryTemplates",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SendWelcomeEmail",
                table: "DirectoryTemplates",
                type: "bit",
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
