using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Database.Migrations.MySql
{
    /// <inheritdoc />
    public partial class Template_Group_Fix_MySql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Create join table
            migrationBuilder.CreateTable(
                name: "DirectoryTemplateDirectoryTemplateGroup",
                columns: table => new
                {
                    AssignedGroupSidsId = table.Column<int>(type: "int", nullable: false),
                    TemplatesId = table.Column<int>(type: "int", nullable: false)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_DirectoryTemplateDirectoryTemplateGroup_TemplatesId",
                table: "DirectoryTemplateDirectoryTemplateGroup",
                column: "TemplatesId");

            // 2) Copy existing relationships into the join table (MySQL syntax)
            migrationBuilder.Sql(@"
INSERT INTO `DirectoryTemplateDirectoryTemplateGroup` (`AssignedGroupSidsId`, `TemplatesId`)
SELECT `Id`, `DirectoryTemplateId`
FROM `DirectoryTemplateGroups`
WHERE `DirectoryTemplateId` IS NOT NULL;
");

            // 3) Now drop the old FK / index / column
            // (keep names used by your generated migration; adjust if DB constraint names differ)
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
                type: "int",
                nullable: true);

            // Copy a single mapping back for each group (if many-to-many existed pick MIN(TemplatesId))
            migrationBuilder.Sql(@"
UPDATE `DirectoryTemplateGroups` g
JOIN (
    SELECT `AssignedGroupSidsId` AS `GroupId`, MIN(`TemplatesId`) AS `TemplateId`
    FROM `DirectoryTemplateDirectoryTemplateGroup`
    GROUP BY `AssignedGroupSidsId`
) m ON g.`Id` = m.`GroupId`
SET g.`DirectoryTemplateId` = m.`TemplateId`;
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
