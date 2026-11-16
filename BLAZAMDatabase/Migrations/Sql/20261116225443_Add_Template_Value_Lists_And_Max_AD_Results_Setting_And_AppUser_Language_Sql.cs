using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Database.Migrations.Sql
{
    /// <inheritdoc />
    public partial class Add_Template_Value_Lists_And_Max_AD_Results_Setting_And_AppUser_Language_Sql : Migration
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

            migrationBuilder.AddColumn<string>(
                name: "PreferredLanguage",
                table: "UserSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ValueListId",
                table: "DirectoryTemplateFieldValues",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "ApiTokenSalt",
                table: "AuthenticationSettings",
                type: "varbinary(max)",
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
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectoryTemplateFieldValueLists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DirectoryTemplateFieldValueListItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StringValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DirectoryTemplateFieldValueListId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectoryTemplateFieldValueListItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DirectoryTemplateFieldValueListItem_DirectoryTemplateFieldValueLists_DirectoryTemplateFieldValueListId",
                        column: x => x.DirectoryTemplateFieldValueListId,
                        principalTable: "DirectoryTemplateFieldValueLists",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "AuthenticationSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "ApiTokenSalt",
                value: null);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Table_Column2",
                table: "EmailSettings",
                sql: "[Id] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_DirectoryTemplateFieldValues_ValueListId",
                table: "DirectoryTemplateFieldValues",
                column: "ValueListId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Table_Column1",
                table: "AuthenticationSettings",
                sql: "[Id] = 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Table_Column",
                table: "AppSettings",
                sql: "[Id] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_DirectoryTemplateFieldValueListItem_DirectoryTemplateFieldValueListId",
                table: "DirectoryTemplateFieldValueListItem",
                column: "DirectoryTemplateFieldValueListId");

            migrationBuilder.AddForeignKey(
                name: "FK_DirectoryTemplateFieldValues_DirectoryTemplateFieldValueLists_ValueListId",
                table: "DirectoryTemplateFieldValues",
                column: "ValueListId",
                principalTable: "DirectoryTemplateFieldValueLists",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DirectoryTemplateFieldValues_DirectoryTemplateFieldValueLists_ValueListId",
                table: "DirectoryTemplateFieldValues");

            migrationBuilder.DropTable(
                name: "DirectoryTemplateFieldValueListItem");

            migrationBuilder.DropTable(
                name: "DirectoryTemplateFieldValueLists");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Table_Column2",
                table: "EmailSettings");

            migrationBuilder.DropIndex(
                name: "IX_DirectoryTemplateFieldValues_ValueListId",
                table: "DirectoryTemplateFieldValues");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Table_Column1",
                table: "AuthenticationSettings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Table_Column",
                table: "AppSettings");

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
        }
    }
}
