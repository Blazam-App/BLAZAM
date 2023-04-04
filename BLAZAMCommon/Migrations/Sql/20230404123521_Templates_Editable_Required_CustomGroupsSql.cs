using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Common.Migrations.Sql
{
    /// <inheritdoc />
    public partial class TemplatesEditableRequiredCustomGroupsSql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowCustomGroups",
                table: "DirectoryTemplates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Editable",
                table: "DirectoryTemplateFieldValues",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Required",
                table: "DirectoryTemplateFieldValues",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowCustomGroups",
                table: "DirectoryTemplates");

            migrationBuilder.DropColumn(
                name: "Editable",
                table: "DirectoryTemplateFieldValues");

            migrationBuilder.DropColumn(
                name: "Required",
                table: "DirectoryTemplateFieldValues");
        }
    }
}
