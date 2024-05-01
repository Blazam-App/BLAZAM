using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Database.Migrations.Sql
{
    /// <inheritdoc />
    public partial class Add_RequirePasswordChange_To_TemplatesSql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequirePasswordChange",
                table: "DirectoryTemplates",
                type: "bit",
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
