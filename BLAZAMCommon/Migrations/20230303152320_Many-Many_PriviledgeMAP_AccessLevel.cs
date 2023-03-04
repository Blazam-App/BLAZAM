using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Common.Migrations
{
    /// <inheritdoc />
    public partial class ManyManyPriviledgeMAPAccessLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessLevels_PrivilegeMap_PrivilegeMapId",
                table: "AccessLevels");

            migrationBuilder.DropIndex(
                name: "IX_AccessLevels_PrivilegeMapId",
                table: "AccessLevels");

            migrationBuilder.DropColumn(
                name: "PrivilegeMapId",
                table: "AccessLevels");

            migrationBuilder.CreateTable(
                name: "AccessLevelPrivilegeMap",
                columns: table => new
                {
                    AccessLevelsAccessLevelId = table.Column<int>(type: "int", nullable: false),
                    PrivilegeMapsPrivilegeMapId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessLevelPrivilegeMap", x => new { x.AccessLevelsAccessLevelId, x.PrivilegeMapsPrivilegeMapId });
                    table.ForeignKey(
                        name: "FK_AccessLevelPrivilegeMap_AccessLevels_AccessLevelsAccessLevelId",
                        column: x => x.AccessLevelsAccessLevelId,
                        principalTable: "AccessLevels",
                        principalColumn: "AccessLevelId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccessLevelPrivilegeMap_PrivilegeMap_PrivilegeMapsPrivilegeMapId",
                        column: x => x.PrivilegeMapsPrivilegeMapId,
                        principalTable: "PrivilegeMap",
                        principalColumn: "PrivilegeMapId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccessLevelPrivilegeMap_PrivilegeMapsPrivilegeMapId",
                table: "AccessLevelPrivilegeMap",
                column: "PrivilegeMapsPrivilegeMapId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessLevelPrivilegeMap");

            migrationBuilder.AddColumn<int>(
                name: "PrivilegeMapId",
                table: "AccessLevels",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AccessLevels",
                keyColumn: "AccessLevelId",
                keyValue: 1,
                column: "PrivilegeMapId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_AccessLevels_PrivilegeMapId",
                table: "AccessLevels",
                column: "PrivilegeMapId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessLevels_PrivilegeMap_PrivilegeMapId",
                table: "AccessLevels",
                column: "PrivilegeMapId",
                principalTable: "PrivilegeMap",
                principalColumn: "PrivilegeMapId");
        }
    }
}
