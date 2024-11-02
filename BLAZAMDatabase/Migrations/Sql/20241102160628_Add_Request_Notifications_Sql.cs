using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Database.Migrations.Sql
{
    /// <inheritdoc />
    public partial class Add_Request_Notifications_Sql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ActionId",
                table: "NotificationMessages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatorId",
                table: "NotificationMessages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MessageType",
                table: "NotificationMessages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TargetDN",
                table: "NotificationMessages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationMessages_ActionId",
                table: "NotificationMessages",
                column: "ActionId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationMessages_CreatorId",
                table: "NotificationMessages",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationMessages_ObjectActionFlag_ActionId",
                table: "NotificationMessages",
                column: "ActionId",
                principalTable: "ObjectActionFlag",
                principalColumn: "Id");

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
                name: "FK_NotificationMessages_ObjectActionFlag_ActionId",
                table: "NotificationMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_NotificationMessages_UserSettings_CreatorId",
                table: "NotificationMessages");

            migrationBuilder.DropIndex(
                name: "IX_NotificationMessages_ActionId",
                table: "NotificationMessages");

            migrationBuilder.DropIndex(
                name: "IX_NotificationMessages_CreatorId",
                table: "NotificationMessages");

            migrationBuilder.DropColumn(
                name: "ActionId",
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
