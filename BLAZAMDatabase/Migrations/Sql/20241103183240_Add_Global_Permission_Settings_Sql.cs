using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Database.Migrations.Sql
{
    /// <inheritdoc />
    public partial class Add_Global_Permission_Settings_Sql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GlobalPermissionSettingsId",
                table: "ObjectActionFlag",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GlobalPermissionSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AllowSelfModification = table.Column<bool>(type: "bit", nullable: false),
                    SelfAccessLevelId = table.Column<int>(type: "int", nullable: true),
                    AllowAccessRequest = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalPermissionSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GlobalPermissionSettings_AccessLevels_SelfAccessLevelId",
                        column: x => x.SelfAccessLevelId,
                        principalTable: "AccessLevels",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "ObjectActionFlag",
                keyColumn: "Id",
                keyValue: 1,
                column: "GlobalPermissionSettingsId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ObjectActionFlag",
                keyColumn: "Id",
                keyValue: 2,
                column: "GlobalPermissionSettingsId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ObjectActionFlag",
                keyColumn: "Id",
                keyValue: 3,
                column: "GlobalPermissionSettingsId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ObjectActionFlag",
                keyColumn: "Id",
                keyValue: 4,
                column: "GlobalPermissionSettingsId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ObjectActionFlag",
                keyColumn: "Id",
                keyValue: 5,
                column: "GlobalPermissionSettingsId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ObjectActionFlag",
                keyColumn: "Id",
                keyValue: 6,
                column: "GlobalPermissionSettingsId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ObjectActionFlag",
                keyColumn: "Id",
                keyValue: 7,
                column: "GlobalPermissionSettingsId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ObjectActionFlag",
                keyColumn: "Id",
                keyValue: 8,
                column: "GlobalPermissionSettingsId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ObjectActionFlag",
                keyColumn: "Id",
                keyValue: 9,
                column: "GlobalPermissionSettingsId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ObjectActionFlag",
                keyColumn: "Id",
                keyValue: 10,
                column: "GlobalPermissionSettingsId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_ObjectActionFlag_GlobalPermissionSettingsId",
                table: "ObjectActionFlag",
                column: "GlobalPermissionSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_GlobalPermissionSettings_SelfAccessLevelId",
                table: "GlobalPermissionSettings",
                column: "SelfAccessLevelId");

            migrationBuilder.AddForeignKey(
                name: "FK_ObjectActionFlag_GlobalPermissionSettings_GlobalPermissionSettingsId",
                table: "ObjectActionFlag",
                column: "GlobalPermissionSettingsId",
                principalTable: "GlobalPermissionSettings",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ObjectActionFlag_GlobalPermissionSettings_GlobalPermissionSettingsId",
                table: "ObjectActionFlag");

            migrationBuilder.DropTable(
                name: "GlobalPermissionSettings");

            migrationBuilder.DropIndex(
                name: "IX_ObjectActionFlag_GlobalPermissionSettingsId",
                table: "ObjectActionFlag");

            migrationBuilder.DropColumn(
                name: "GlobalPermissionSettingsId",
                table: "ObjectActionFlag");
        }
    }
}
