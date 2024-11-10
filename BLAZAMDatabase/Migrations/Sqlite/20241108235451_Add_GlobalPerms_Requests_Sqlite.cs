using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class Add_GlobalPerms_Requests_Sqlite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Action",
                table: "NotificationMessages",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatorId",
                table: "NotificationMessages",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MessageType",
                table: "NotificationMessages",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TargetDN",
                table: "NotificationMessages",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GlobalPermissionRequestActions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ObjectAction = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalPermissionRequestActions", x => x.Id);
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
                name: "IX_NotificationMessages_CreatorId",
                table: "NotificationMessages",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationMessages_UserSettings_CreatorId",
                table: "NotificationMessages",
                column: "CreatorId",
                principalTable: "UserSettings",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationMessages_UserSettings_CreatorId",
                table: "NotificationMessages");

            migrationBuilder.DropTable(
                name: "GlobalPermissionRequestActions");

            migrationBuilder.DropTable(
                name: "GlobalPermissionSettings");

            migrationBuilder.DropIndex(
                name: "IX_NotificationMessages_CreatorId",
                table: "NotificationMessages");

            migrationBuilder.DropColumn(
                name: "Action",
                table: "NotificationMessages");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "NotificationMessages");

            migrationBuilder.DropColumn(
                name: "MessageType",
                table: "NotificationMessages");

            migrationBuilder.DropColumn(
                name: "TargetDN",
                table: "NotificationMessages");
        }
    }
}
