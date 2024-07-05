using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Database.Migrations.Sql
{
    /// <inheritdoc />
    public partial class Add_DelegateName_And_EmailTemplateChanges_Sql : Migration
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
                type: "nvarchar(max)",
                nullable: true);

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
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CC",
                table: "EmailTemplates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TemplateName",
                table: "EmailTemplates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
