using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Database.Migrations.Sql
{
    /// <inheritdoc />
    public partial class Rename_And_Add_FieldBool_Sql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AllowAccessRequest",
                table: "GlobalPermissionSettings",
                newName: "AllowActionAccessRequest");

            migrationBuilder.AddColumn<bool>(
                name: "AllowFieldAccessRequest",
                table: "GlobalPermissionSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowFieldAccessRequest",
                table: "GlobalPermissionSettings");

            migrationBuilder.RenameColumn(
                name: "AllowActionAccessRequest",
                table: "GlobalPermissionSettings",
                newName: "AllowAccessRequest");
        }
    }
}
