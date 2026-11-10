using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class Add_Template_Value_Lists_Sqlite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreferredLanguage",
                table: "UserSettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ValueListId",
                table: "DirectoryTemplateFieldValues",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "ApiTokenSalt",
                table: "AuthenticationSettings",
                type: "BLOB",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DirectoryTemplateFieldValueList",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectoryTemplateFieldValueList", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DirectoryTemplateFieldValueListItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    DirectoryTemplateFieldValueListId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectoryTemplateFieldValueListItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DirectoryTemplateFieldValueListItem_DirectoryTemplateFieldValueList_DirectoryTemplateFieldValueListId",
                        column: x => x.DirectoryTemplateFieldValueListId,
                        principalTable: "DirectoryTemplateFieldValueList",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "AuthenticationSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "ApiTokenSalt",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_DirectoryTemplateFieldValues_ValueListId",
                table: "DirectoryTemplateFieldValues",
                column: "ValueListId");

            migrationBuilder.CreateIndex(
                name: "IX_DirectoryTemplateFieldValueListItem_DirectoryTemplateFieldValueListId",
                table: "DirectoryTemplateFieldValueListItem",
                column: "DirectoryTemplateFieldValueListId");

            migrationBuilder.AddForeignKey(
                name: "FK_DirectoryTemplateFieldValues_DirectoryTemplateFieldValueList_ValueListId",
                table: "DirectoryTemplateFieldValues",
                column: "ValueListId",
                principalTable: "DirectoryTemplateFieldValueList",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DirectoryTemplateFieldValues_DirectoryTemplateFieldValueList_ValueListId",
                table: "DirectoryTemplateFieldValues");

            migrationBuilder.DropTable(
                name: "DirectoryTemplateFieldValueListItem");

            migrationBuilder.DropTable(
                name: "DirectoryTemplateFieldValueList");

            migrationBuilder.DropIndex(
                name: "IX_DirectoryTemplateFieldValues_ValueListId",
                table: "DirectoryTemplateFieldValues");

            migrationBuilder.DropColumn(
                name: "PreferredLanguage",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "ValueListId",
                table: "DirectoryTemplateFieldValues");

            migrationBuilder.DropColumn(
                name: "ApiTokenSalt",
                table: "AuthenticationSettings");
        }
    }
}
