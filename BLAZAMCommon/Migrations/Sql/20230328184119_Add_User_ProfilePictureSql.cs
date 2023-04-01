using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Common.Migrations.Sql
{
    /// <inheritdoc />
    public partial class AddUserProfilePictureSql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Table_Column3",
                table: "EmailSettings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Table_Column2",
                table: "AuthenticationSettings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Table_Column1",
                table: "AppSettings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Table_Column",
                table: "ActiveDirectorySettings");

            migrationBuilder.AddColumn<byte[]>(
                name: "ProfilePicture",
                table: "UserSettings",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "AutoUpdateTime",
                table: "AppSettings",
                type: "time",
                nullable: true,
                oldClrType: typeof(TimeSpan),
                oldType: "time");

            migrationBuilder.CreateTable(
                name: "UserNotifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Link = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Expires = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Dismissable = table.Column<bool>(type: "bit", nullable: false),
                    AppUserId = table.Column<int>(type: "int", nullable: true)
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
                name: "CK_Table_Column3",
                table: "EmailSettings",
                sql: "[Id] = 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Table_Column2",
                table: "AuthenticationSettings",
                sql: "[Id] = 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Table_Column1",
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
                name: "CK_Table_Column3",
                table: "EmailSettings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Table_Column2",
                table: "AuthenticationSettings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Table_Column1",
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
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0),
                oldClrType: typeof(TimeSpan),
                oldType: "time",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "DisplayName", "FieldName" },
                values: new object[] { "Street", "street" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Table_Column3",
                table: "EmailSettings",
                sql: "[EmailSettingsId] = 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Table_Column2",
                table: "AuthenticationSettings",
                sql: "[AuthenticationSettingsId] = 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Table_Column1",
                table: "AppSettings",
                sql: "[AppSettingsId] = 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Table_Column",
                table: "ActiveDirectorySettings",
                sql: "[ADSettingsId] = 1");
        }
    }
}
