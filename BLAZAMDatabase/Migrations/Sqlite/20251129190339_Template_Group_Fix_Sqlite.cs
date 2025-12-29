using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class Template_Group_Fix_Sqlite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Create join table first
            migrationBuilder.CreateTable(
                name: "DirectoryTemplateDirectoryTemplateGroup",
                columns: table => new
                {
                    AssignedGroupSidsId = table.Column<int>(type: "INTEGER", nullable: false),
                    TemplatesId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectoryTemplateDirectoryTemplateGroup", x => new { x.AssignedGroupSidsId, x.TemplatesId });
                    table.ForeignKey(
                        name: "FK_DirectoryTemplateDirectoryTemplateGroup_DirectoryTemplateGroups_AssignedGroupSidsId",
                        column: x => x.AssignedGroupSidsId,
                        principalTable: "DirectoryTemplateGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DirectoryTemplateDirectoryTemplateGroup_DirectoryTemplates_TemplatesId",
                        column: x => x.TemplatesId,
                        principalTable: "DirectoryTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DirectoryTemplateDirectoryTemplateGroup_TemplatesId",
                table: "DirectoryTemplateDirectoryTemplateGroup",
                column: "TemplatesId");

            // 2) Copy existing relationships into the join table (SQLite syntax)
            migrationBuilder.Sql(@"
INSERT INTO ""DirectoryTemplateDirectoryTemplateGroup"" (""AssignedGroupSidsId"", ""TemplatesId"")
SELECT ""Id"", ""DirectoryTemplateId""
FROM ""DirectoryTemplateGroups""
WHERE ""DirectoryTemplateId"" IS NOT NULL;
");

            // 3) Now drop the old FK / index / column (after data copied)
            migrationBuilder.DropForeignKey(
                name: "FK_DirectoryTemplateGroups_DirectoryTemplates_DirectoryTemplateId",
                table: "DirectoryTemplateGroups");

            migrationBuilder.DropIndex(
                name: "IX_DirectoryTemplateGroups_DirectoryTemplateId",
                table: "DirectoryTemplateGroups");

            migrationBuilder.DropColumn(
                name: "DirectoryTemplateId",
                table: "DirectoryTemplateGroups");

            // keep the other provider-specific changes originally generated
            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 41,
                column: "FieldType",
                value: 5);

            migrationBuilder.InsertData(
                table: "ActiveDirectoryFields",
                columns: new[] { "Id", "DisplayName", "FieldName", "FieldType", "PropertyName" },
                values: new object[] { 45, "LAPS Password", "msLAPS-Password", 0, "LapsPassword" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore the old column
            migrationBuilder.AddColumn<int>(
                name: "DirectoryTemplateId",
                table: "DirectoryTemplateGroups",
                type: "INTEGER",
                nullable: true);

            // Copy a single mapping back for each group (if many-to-many existed pick MIN(TemplatesId))
            migrationBuilder.Sql(@"
UPDATE ""DirectoryTemplateGroups""
SET ""DirectoryTemplateId"" = (
    SELECT MIN(""TemplatesId"")
    FROM ""DirectoryTemplateDirectoryTemplateGroup""
    WHERE ""AssignedGroupSidsId"" = ""DirectoryTemplateGroups"".""Id""
)
WHERE EXISTS (
    SELECT 1
    FROM ""DirectoryTemplateDirectoryTemplateGroup""
    WHERE ""AssignedGroupSidsId"" = ""DirectoryTemplateGroups"".""Id""
);
");

            // Drop join table
            migrationBuilder.DropTable(
                name: "DirectoryTemplateDirectoryTemplateGroup");

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 41,
                column: "FieldType",
                value: 6);

            migrationBuilder.CreateIndex(
                name: "IX_DirectoryTemplateGroups_DirectoryTemplateId",
                table: "DirectoryTemplateGroups",
                column: "DirectoryTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_DirectoryTemplateGroups_DirectoryTemplates_DirectoryTemplateId",
                table: "DirectoryTemplateGroups",
                column: "DirectoryTemplateId",
                principalTable: "DirectoryTemplates",
                principalColumn: "Id");
        }
    }
}
