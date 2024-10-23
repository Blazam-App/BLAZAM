using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Database.Migrations.MySql
{
    /// <inheritdoc />
    public partial class Add_Allow_Template_Username_Override_MySql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowUsernameOverride",
                table: "DirectoryTemplates",
                type: "tinyint(1)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowUsernameOverride",
                table: "DirectoryTemplates");
        }
    }
}
