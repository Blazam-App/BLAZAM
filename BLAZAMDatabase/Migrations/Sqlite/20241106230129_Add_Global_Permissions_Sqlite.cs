using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class Add_Global_Permissions_Sqlite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GlobalPermissionRequestActions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ObjectActionId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalPermissionRequestActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GlobalPermissionRequestActions_ObjectActionFlag_ObjectActionId",
                        column: x => x.ObjectActionId,
                        principalTable: "ObjectActionFlag",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GlobalPermissionSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AllowSelfModification = table.Column<bool>(type: "INTEGER", nullable: false),
                    AllowAccessRequest = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalPermissionSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GlobalPermissionRequestActions_ObjectActionId",
                table: "GlobalPermissionRequestActions",
                column: "ObjectActionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GlobalPermissionRequestActions");

            migrationBuilder.DropTable(
                name: "GlobalPermissionSettings");
        }
    }
}
