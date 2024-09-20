using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace BLAZAM.Common.Migrations.MySql
{
    /// <inheritdoc />
    public partial class RecoverableTemplatesFieldsPermissionsMySql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "DirectoryTemplates",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "CustomActiveDirectoryFields",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "AccessLevelObjectMapping",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "DirectoryTemplates");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "CustomActiveDirectoryFields");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "AccessLevelObjectMapping");
        }
    }
}
