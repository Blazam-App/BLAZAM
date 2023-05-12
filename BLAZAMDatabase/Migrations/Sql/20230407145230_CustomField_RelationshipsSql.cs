﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Common.Migrations.Sql
{
    /// <inheritdoc />
    public partial class CustomFieldRelationshipsSql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DirectoryTemplateFieldValues_ActiveDirectoryFields_FieldId",
                table: "DirectoryTemplateFieldValues");

            migrationBuilder.AlterColumn<int>(
                name: "FieldId",
                table: "DirectoryTemplateFieldValues",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "CustomFieldId",
                table: "DirectoryTemplateFieldValues",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AdminPassword",
                table: "AuthenticationSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CustomFieldId",
                table: "AccessLevelFieldMapping",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AuthenticationSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "SessionTimeout",
                value: 900);

            migrationBuilder.CreateIndex(
                name: "IX_DirectoryTemplateFieldValues_CustomFieldId",
                table: "DirectoryTemplateFieldValues",
                column: "CustomFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessLevelFieldMapping_CustomFieldId",
                table: "AccessLevelFieldMapping",
                column: "CustomFieldId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessLevelFieldMapping_CustomActiveDirectoryFields_CustomFieldId",
                table: "AccessLevelFieldMapping",
                column: "CustomFieldId",
                principalTable: "CustomActiveDirectoryFields",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DirectoryTemplateFieldValues_ActiveDirectoryFields_FieldId",
                table: "DirectoryTemplateFieldValues",
                column: "FieldId",
                principalTable: "ActiveDirectoryFields",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DirectoryTemplateFieldValues_CustomActiveDirectoryFields_CustomFieldId",
                table: "DirectoryTemplateFieldValues",
                column: "CustomFieldId",
                principalTable: "CustomActiveDirectoryFields",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessLevelFieldMapping_CustomActiveDirectoryFields_CustomFieldId",
                table: "AccessLevelFieldMapping");

            migrationBuilder.DropForeignKey(
                name: "FK_DirectoryTemplateFieldValues_ActiveDirectoryFields_FieldId",
                table: "DirectoryTemplateFieldValues");

            migrationBuilder.DropForeignKey(
                name: "FK_DirectoryTemplateFieldValues_CustomActiveDirectoryFields_CustomFieldId",
                table: "DirectoryTemplateFieldValues");

            migrationBuilder.DropIndex(
                name: "IX_DirectoryTemplateFieldValues_CustomFieldId",
                table: "DirectoryTemplateFieldValues");

            migrationBuilder.DropIndex(
                name: "IX_AccessLevelFieldMapping_CustomFieldId",
                table: "AccessLevelFieldMapping");

            migrationBuilder.DropColumn(
                name: "CustomFieldId",
                table: "DirectoryTemplateFieldValues");

            migrationBuilder.DropColumn(
                name: "CustomFieldId",
                table: "AccessLevelFieldMapping");

            migrationBuilder.AlterColumn<int>(
                name: "FieldId",
                table: "DirectoryTemplateFieldValues",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AdminPassword",
                table: "AuthenticationSettings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.UpdateData(
                table: "AuthenticationSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "SessionTimeout",
                value: null);

            migrationBuilder.AddForeignKey(
                name: "FK_DirectoryTemplateFieldValues_ActiveDirectoryFields_FieldId",
                table: "DirectoryTemplateFieldValues",
                column: "FieldId",
                principalTable: "ActiveDirectoryFields",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
