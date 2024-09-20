using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace BLAZAM.Common.Migrations.Sql
{
    /// <inheritdoc />
    public partial class RecoverableTemplatesFieldsPermissionsSql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "DirectoryTemplates",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "CustomActiveDirectoryFields",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "AccessLevelObjectMapping",
                type: "datetime2",
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
