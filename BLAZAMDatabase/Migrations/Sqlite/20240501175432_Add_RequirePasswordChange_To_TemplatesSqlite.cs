using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class Add_RequirePasswordChange_To_TemplatesSqlite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequirePasswordChange",
                table: "DirectoryTemplates",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequirePasswordChange",
                table: "DirectoryTemplates");
        }
    }
}
