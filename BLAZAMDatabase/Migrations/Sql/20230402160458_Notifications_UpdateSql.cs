using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace BLAZAM.Common.Migrations.Sql
{
    /// <inheritdoc />
    public partial class NotificationsUpdateSql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserNotifications_UserSettings_AppUserId",
                table: "UserNotifications");

            migrationBuilder.DropIndex(
                name: "IX_UserNotifications_AppUserId",
                table: "UserNotifications");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "UserNotifications");

            migrationBuilder.DropColumn(
                name: "Expires",
                table: "UserNotifications");

            migrationBuilder.DropColumn(
                name: "Link",
                table: "UserNotifications");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "UserNotifications");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "UserNotifications");

            migrationBuilder.RenameColumn(
                name: "Level",
                table: "UserNotifications",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "Dismissable",
                table: "UserNotifications",
                newName: "IsRead");

            migrationBuilder.AddColumn<int>(
                name: "NotificationId",
                table: "UserNotifications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "NotificationMessage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Link = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Expires = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Dismissable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationMessage", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_NotificationId",
                table: "UserNotifications",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_UserId",
                table: "UserNotifications",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserNotifications_NotificationMessage_NotificationId",
                table: "UserNotifications",
                column: "NotificationId",
                principalTable: "NotificationMessage",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserNotifications_UserSettings_UserId",
                table: "UserNotifications",
                column: "UserId",
                principalTable: "UserSettings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserNotifications_NotificationMessage_NotificationId",
                table: "UserNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_UserNotifications_UserSettings_UserId",
                table: "UserNotifications");

            migrationBuilder.DropTable(
                name: "NotificationMessage");

            migrationBuilder.DropIndex(
                name: "IX_UserNotifications_NotificationId",
                table: "UserNotifications");

            migrationBuilder.DropIndex(
                name: "IX_UserNotifications_UserId",
                table: "UserNotifications");

            migrationBuilder.DropColumn(
                name: "NotificationId",
                table: "UserNotifications");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "UserNotifications",
                newName: "Level");

            migrationBuilder.RenameColumn(
                name: "IsRead",
                table: "UserNotifications",
                newName: "Dismissable");

            migrationBuilder.AddColumn<int>(
                name: "AppUserId",
                table: "UserNotifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Expires",
                table: "UserNotifications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Link",
                table: "UserNotifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "UserNotifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "UserNotifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_AppUserId",
                table: "UserNotifications",
                column: "AppUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserNotifications_UserSettings_AppUserId",
                table: "UserNotifications",
                column: "AppUserId",
                principalTable: "UserSettings",
                principalColumn: "Id");
        }
    }
}
