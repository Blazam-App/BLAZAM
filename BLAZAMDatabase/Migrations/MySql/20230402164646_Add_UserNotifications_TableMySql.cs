using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Common.Migrations.MySql
{
    /// <inheritdoc />
    public partial class AddUserNotificationsTableMySql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserNotifications_NotificationMessage_NotificationId",
                table: "UserNotifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NotificationMessage",
                table: "NotificationMessage");

            migrationBuilder.RenameTable(
                name: "NotificationMessage",
                newName: "NotificationMessages");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NotificationMessages",
                table: "NotificationMessages",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserNotifications_NotificationMessages_NotificationId",
                table: "UserNotifications",
                column: "NotificationId",
                principalTable: "NotificationMessages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserNotifications_NotificationMessages_NotificationId",
                table: "UserNotifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NotificationMessages",
                table: "NotificationMessages");

            migrationBuilder.RenameTable(
                name: "NotificationMessages",
                newName: "NotificationMessage");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NotificationMessage",
                table: "NotificationMessage",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserNotifications_NotificationMessage_NotificationId",
                table: "UserNotifications",
                column: "NotificationId",
                principalTable: "NotificationMessage",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
