using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Common.Migrations
{
    /// <inheritdoc />
    public partial class addPasswordToTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EmailAddressFormula",
                table: "DirectoryTemplates",
                newName: "PasswordFormula");

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "DirectoryTemplateFieldValues",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PasswordFormula",
                table: "DirectoryTemplates",
                newName: "EmailAddressFormula");

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "DirectoryTemplateFieldValues",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
