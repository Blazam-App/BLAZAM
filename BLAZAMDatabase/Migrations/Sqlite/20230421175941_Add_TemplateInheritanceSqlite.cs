using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class Add_TemplateInheritanceSqlite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentTemplateId",
                table: "DirectoryTemplates",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DirectoryTemplates_ParentTemplateId",
                table: "DirectoryTemplates",
                column: "ParentTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_DirectoryTemplates_DirectoryTemplates_ParentTemplateId",
                table: "DirectoryTemplates",
                column: "ParentTemplateId",
                principalTable: "DirectoryTemplates",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DirectoryTemplates_DirectoryTemplates_ParentTemplateId",
                table: "DirectoryTemplates");

            migrationBuilder.DropIndex(
                name: "IX_DirectoryTemplates_ParentTemplateId",
                table: "DirectoryTemplates");

            migrationBuilder.DropColumn(
                name: "ParentTemplateId",
                table: "DirectoryTemplates");
        }
    }
}
