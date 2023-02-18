using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Common.Migrations
{
    /// <inheritdoc />
    public partial class templateCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "DirectoryTemplates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "ActiveDirectoryFieldId",
                keyValue: 27,
                column: "DisplayName",
                value: "Canonical Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "DirectoryTemplates");

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "ActiveDirectoryFieldId",
                keyValue: 27,
                column: "DisplayName",
                value: "Container Name");
        }
    }
}
