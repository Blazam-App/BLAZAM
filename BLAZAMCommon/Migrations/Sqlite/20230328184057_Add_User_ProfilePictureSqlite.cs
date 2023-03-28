using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Common.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class AddUserProfilePictureSqlite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Table_Column",
                table: "EmailSettings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Table_Column",
                table: "AuthenticationSettings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Table_Column",
                table: "AppSettings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Table_Column",
                table: "ActiveDirectorySettings");

            migrationBuilder.AddColumn<byte[]>(
                name: "ProfilePicture",
                table: "UserSettings",
                type: "BLOB",
                nullable: true);

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "AutoUpdateTime",
                table: "AppSettings",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(TimeSpan),
                oldType: "TEXT");

            migrationBuilder.CreateTable(
                name: "UserNotifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    Message = table.Column<string>(type: "TEXT", nullable: true),
                    Link = table.Column<string>(type: "TEXT", nullable: true),
                    Expires = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Dismissable = table.Column<bool>(type: "INTEGER", nullable: false),
                    AppUserId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserNotifications_UserSettings_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "UserSettings",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "DisplayName", "FieldName" },
                values: new object[] { "PO Box", "postOfficeBox" });

            migrationBuilder.InsertData(
                table: "ActiveDirectoryFields",
                columns: new[] { "Id", "DisplayName", "FieldName" },
                values: new object[] { 34, "Manager", "manager" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Table_Column",
                table: "EmailSettings",
                sql: "[Id] = 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Table_Column",
                table: "AuthenticationSettings",
                sql: "[Id] = 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Table_Column",
                table: "AppSettings",
                sql: "[Id] = 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Table_Column",
                table: "ActiveDirectorySettings",
                sql: "[Id] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_AppUserId",
                table: "UserNotifications",
                column: "AppUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserNotifications");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Table_Column",
                table: "EmailSettings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Table_Column",
                table: "AuthenticationSettings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Table_Column",
                table: "AppSettings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Table_Column",
                table: "ActiveDirectorySettings");

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DropColumn(
                name: "ProfilePicture",
                table: "UserSettings");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "AutoUpdateTime",
                table: "AppSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0),
                oldClrType: typeof(TimeSpan),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "DisplayName", "FieldName" },
                values: new object[] { "Street", "street" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Table_Column",
                table: "EmailSettings",
                sql: "[EmailSettingsId] = 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Table_Column",
                table: "AuthenticationSettings",
                sql: "[AuthenticationSettingsId] = 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Table_Column",
                table: "AppSettings",
                sql: "[AppSettingsId] = 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Table_Column",
                table: "ActiveDirectorySettings",
                sql: "[ADSettingsId] = 1");
        }
    }
}
