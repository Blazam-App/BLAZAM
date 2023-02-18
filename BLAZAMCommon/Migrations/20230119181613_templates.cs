using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Common.Migrations
{
    /// <inheritdoc />
    public partial class templates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DirectoryTemplates",
                columns: table => new
                {
                    DirectoryTemplateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ObjectType = table.Column<int>(type: "int", nullable: false),
                    DisplayNameFormula = table.Column<int>(type: "int", nullable: false),
                    UsernameFormula = table.Column<int>(type: "int", nullable: false),
                    MatchedUsernameResolution = table.Column<int>(type: "int", nullable: false),
                    SwapUsernamesOnDisabledMatch = table.Column<bool>(type: "bit", nullable: false),
                    ParentOU = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectoryTemplates", x => x.DirectoryTemplateId);
                });

            migrationBuilder.CreateTable(
                name: "DirectoryTemplateFieldValues",
                columns: table => new
                {
                    DirectoryTemplateFieldValueId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FieldActiveDirectoryFieldId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DirectoryTemplateId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectoryTemplateFieldValues", x => x.DirectoryTemplateFieldValueId);
                    table.ForeignKey(
                        name: "FK_DirectoryTemplateFieldValues_ActiveDirectoryFields_FieldActiveDirectoryFieldId",
                        column: x => x.FieldActiveDirectoryFieldId,
                        principalTable: "ActiveDirectoryFields",
                        principalColumn: "ActiveDirectoryFieldId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DirectoryTemplateFieldValues_DirectoryTemplates_DirectoryTemplateId",
                        column: x => x.DirectoryTemplateId,
                        principalTable: "DirectoryTemplates",
                        principalColumn: "DirectoryTemplateId");
                });

            migrationBuilder.CreateTable(
                name: "DirectoryTemplateGroups",
                columns: table => new
                {
                    DirectoryTemplateGroupId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupSid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DirectoryTemplateId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectoryTemplateGroups", x => x.DirectoryTemplateGroupId);
                    table.ForeignKey(
                        name: "FK_DirectoryTemplateGroups_DirectoryTemplates_DirectoryTemplateId",
                        column: x => x.DirectoryTemplateId,
                        principalTable: "DirectoryTemplates",
                        principalColumn: "DirectoryTemplateId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DirectoryTemplateFieldValues_DirectoryTemplateId",
                table: "DirectoryTemplateFieldValues",
                column: "DirectoryTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_DirectoryTemplateFieldValues_FieldActiveDirectoryFieldId",
                table: "DirectoryTemplateFieldValues",
                column: "FieldActiveDirectoryFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_DirectoryTemplateGroups_DirectoryTemplateId",
                table: "DirectoryTemplateGroups",
                column: "DirectoryTemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DirectoryTemplateFieldValues");

            migrationBuilder.DropTable(
                name: "DirectoryTemplateGroups");

            migrationBuilder.DropTable(
                name: "DirectoryTemplates");
        }
    }
}
