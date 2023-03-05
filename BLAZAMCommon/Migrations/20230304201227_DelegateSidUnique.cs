using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Common.Migrations
{
    /// <inheritdoc />
    public partial class DelegateSidUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "DelegateSid",
                table: "PermissionDelegate",
                type: "varbinary(900)",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionDelegate_DelegateSid",
                table: "PermissionDelegate",
                column: "DelegateSid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PermissionDelegate_DelegateSid",
                table: "PermissionDelegate");

            migrationBuilder.AlterColumn<byte[]>(
                name: "DelegateSid",
                table: "PermissionDelegate",
                type: "varbinary(max)",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(900)");
        }
    }
}
