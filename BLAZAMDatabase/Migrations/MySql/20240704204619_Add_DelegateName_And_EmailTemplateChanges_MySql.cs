using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Database.Migrations.MySql
{
    /// <inheritdoc />
    public partial class Add_DelegateName_And_EmailTemplateChanges_MySql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BCC",
                table: "EmailTemplates");

            migrationBuilder.DropColumn(
                name: "CC",
                table: "EmailTemplates");

            migrationBuilder.DropColumn(
                name: "TemplateName",
                table: "EmailTemplates");

            migrationBuilder.AddColumn<string>(
                name: "DelegateName",
                table: "PermissionDelegate",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "TemplateType",
                table: "EmailTemplates",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DelegateName",
                table: "PermissionDelegate");

            migrationBuilder.DropColumn(
                name: "TemplateType",
                table: "EmailTemplates");

            migrationBuilder.AddColumn<string>(
                name: "BCC",
                table: "EmailTemplates",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CC",
                table: "EmailTemplates",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TemplateName",
                table: "EmailTemplates",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
