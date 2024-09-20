using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace BLAZAM.Common.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class FixFieldAccessMappingRequiredADFieldSqlite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessLevelFieldMapping_ActiveDirectoryFields_ActiveDirectoryFieldId",
                table: "AccessLevelFieldMapping");

            migrationBuilder.DropIndex(
                name: "IX_AccessLevelFieldMapping_ActiveDirectoryFieldId",
                table: "AccessLevelFieldMapping");

            migrationBuilder.DropColumn(
                name: "ActiveDirectoryFieldId",
                table: "AccessLevelFieldMapping");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "NotificationMessages",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FieldId",
                table: "AccessLevelFieldMapping",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.InsertData(
                table: "ActiveDirectoryFields",
                columns: new[] { "Id", "DisplayName", "FieldName", "FieldType" },
                values: new object[] { 35, "Photo", "thumbnail", 2 });

            migrationBuilder.CreateIndex(
                name: "IX_AccessLevelFieldMapping_FieldId",
                table: "AccessLevelFieldMapping",
                column: "FieldId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessLevelFieldMapping_ActiveDirectoryFields_FieldId",
                table: "AccessLevelFieldMapping",
                column: "FieldId",
                principalTable: "ActiveDirectoryFields",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessLevelFieldMapping_ActiveDirectoryFields_FieldId",
                table: "AccessLevelFieldMapping");

            migrationBuilder.DropIndex(
                name: "IX_AccessLevelFieldMapping_FieldId",
                table: "AccessLevelFieldMapping");

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DropColumn(
                name: "FieldId",
                table: "AccessLevelFieldMapping");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "NotificationMessages",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AddColumn<int>(
                name: "ActiveDirectoryFieldId",
                table: "AccessLevelFieldMapping",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AccessLevelFieldMapping_ActiveDirectoryFieldId",
                table: "AccessLevelFieldMapping",
                column: "ActiveDirectoryFieldId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessLevelFieldMapping_ActiveDirectoryFields_ActiveDirectoryFieldId",
                table: "AccessLevelFieldMapping",
                column: "ActiveDirectoryFieldId",
                principalTable: "ActiveDirectoryFields",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
