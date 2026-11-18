using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Database.Migrations.MySql
{
    /// <inheritdoc />
    public partial class Add_Template_Value_Lists_And_Max_AD_Results_Setting_And_AppUser_Language_MySql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreferredLanguage",
                table: "UserSettings",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "ValueListId",
                table: "DirectoryTemplateFieldValues",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "ApiTokenSalt",
                table: "AuthenticationSettings",
                type: "longblob",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxSearchResults",
                table: "ActiveDirectorySettings",
                type: "int",
                nullable: false,
                defaultValue: 3000);

            migrationBuilder.CreateTable(
                name: "DirectoryTemplateFieldValueLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectoryTemplateFieldValueLists", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DirectoryTemplateFieldValueListItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    StringValue = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DirectoryTemplateFieldValueListId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectoryTemplateFieldValueListItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DirectoryTemplateFieldValueListItem_DirectoryTemplateFieldVa~",
                        column: x => x.DirectoryTemplateFieldValueListId,
                        principalTable: "DirectoryTemplateFieldValueLists",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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
                name: "IX_DirectoryTemplateFieldValueListItem_DirectoryTemplateFieldVa~",
                table: "DirectoryTemplateFieldValueListItem",
                column: "DirectoryTemplateFieldValueListId");

            migrationBuilder.AddForeignKey(
                name: "FK_DirectoryTemplateFieldValues_DirectoryTemplateFieldValueList~",
                table: "DirectoryTemplateFieldValues",
                column: "ValueListId",
                principalTable: "DirectoryTemplateFieldValueLists",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DirectoryTemplateFieldValues_DirectoryTemplateFieldValueList~",
                table: "DirectoryTemplateFieldValues");

            migrationBuilder.DropTable(
                name: "DirectoryTemplateFieldValueListItem");

            migrationBuilder.DropTable(
                name: "DirectoryTemplateFieldValueLists");

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

            migrationBuilder.DropColumn(
                name: "MaxSearchResults",
                table: "ActiveDirectorySettings");
        }
    }
}
