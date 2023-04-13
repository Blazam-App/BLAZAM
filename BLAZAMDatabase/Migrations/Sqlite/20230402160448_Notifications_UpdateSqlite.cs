using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Common.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class NotificationsUpdateSqlite : Migration
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
                newName: "NotificationId");

            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                table: "UserNotifications",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "NotificationMessage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    Message = table.Column<string>(type: "TEXT", nullable: true),
                    Link = table.Column<string>(type: "TEXT", nullable: true),
                    Expires = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Dismissable = table.Column<bool>(type: "INTEGER", nullable: false)
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
                name: "IsRead",
                table: "UserNotifications");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "UserNotifications",
                newName: "Level");

            migrationBuilder.RenameColumn(
                name: "NotificationId",
                table: "UserNotifications",
                newName: "Dismissable");

            migrationBuilder.AddColumn<int>(
                name: "AppUserId",
                table: "UserNotifications",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Expires",
                table: "UserNotifications",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Link",
                table: "UserNotifications",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "UserNotifications",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "UserNotifications",
                type: "TEXT",
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
