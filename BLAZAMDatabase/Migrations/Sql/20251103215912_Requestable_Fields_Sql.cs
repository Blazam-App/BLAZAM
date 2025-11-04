using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Database.Migrations.Sql
{
    /// <inheritdoc />
    public partial class Requestable_Fields_Sql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GlobalPermissionRequestField",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AllowEdit = table.Column<bool>(type: "bit", nullable: false),
                    FieldId = table.Column<int>(type: "int", nullable: true),
                    CustomFieldId = table.Column<int>(type: "int", nullable: true),
                    GlobalPermissionSettingsId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalPermissionRequestField", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GlobalPermissionRequestField_ActiveDirectoryFields_FieldId",
                        column: x => x.FieldId,
                        principalTable: "ActiveDirectoryFields",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GlobalPermissionRequestField_CustomActiveDirectoryFields_CustomFieldId",
                        column: x => x.CustomFieldId,
                        principalTable: "CustomActiveDirectoryFields",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GlobalPermissionRequestField_GlobalPermissionSettings_GlobalPermissionSettingsId",
                        column: x => x.GlobalPermissionSettingsId,
                        principalTable: "GlobalPermissionSettings",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationMessages_CustomFieldId",
                table: "NotificationMessages",
                column: "CustomFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationMessages_FieldId",
                table: "NotificationMessages",
                column: "FieldId");

            migrationBuilder.CreateIndex(
                name: "IX_GlobalPermissionRequestField_CustomFieldId",
                table: "GlobalPermissionRequestField",
                column: "CustomFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_GlobalPermissionRequestField_FieldId",
                table: "GlobalPermissionRequestField",
                column: "FieldId");

            migrationBuilder.CreateIndex(
                name: "IX_GlobalPermissionRequestField_GlobalPermissionSettingsId",
                table: "GlobalPermissionRequestField",
                column: "GlobalPermissionSettingsId");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationMessages_ActiveDirectoryFields_FieldId",
                table: "NotificationMessages",
                column: "FieldId",
                principalTable: "ActiveDirectoryFields",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationMessages_CustomActiveDirectoryFields_CustomFieldId",
                table: "NotificationMessages",
                column: "CustomFieldId",
                principalTable: "CustomActiveDirectoryFields",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationMessages_ActiveDirectoryFields_FieldId",
                table: "NotificationMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_NotificationMessages_CustomActiveDirectoryFields_CustomFieldId",
                table: "NotificationMessages");

            migrationBuilder.DropTable(
                name: "GlobalPermissionRequestField");

            migrationBuilder.DropIndex(
                name: "IX_NotificationMessages_CustomFieldId",
                table: "NotificationMessages");

            migrationBuilder.DropIndex(
                name: "IX_NotificationMessages_FieldId",
                table: "NotificationMessages");
        }
    }
}
