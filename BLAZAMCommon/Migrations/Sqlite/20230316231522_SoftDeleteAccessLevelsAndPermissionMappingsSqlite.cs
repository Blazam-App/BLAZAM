using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Common.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class SoftDeleteAccessLevelsAndPermissionMappingsSqlite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessLevelFieldAccessMapping_AccessLevelFieldMapping_FieldMapFieldAccessMappingId",
                table: "AccessLevelFieldAccessMapping");

            migrationBuilder.DropForeignKey(
                name: "FK_AccessLevelFieldAccessMapping_AccessLevels_AccessLevelsAccessLevelId",
                table: "AccessLevelFieldAccessMapping");

            migrationBuilder.DropForeignKey(
                name: "FK_AccessLevelObjectAccessMapping_AccessLevelObjectMapping_ObjectMapObjectAccessMappingId",
                table: "AccessLevelObjectAccessMapping");

            migrationBuilder.DropForeignKey(
                name: "FK_AccessLevelObjectAccessMapping_AccessLevels_AccessLevelsAccessLevelId",
                table: "AccessLevelObjectAccessMapping");

            migrationBuilder.DropForeignKey(
                name: "FK_ActionAccessMapping_ObjectActionFlag_ObjectActionActionAccessFlagId",
                table: "ActionAccessMapping");

            migrationBuilder.DropForeignKey(
                name: "FK_DirectoryTemplateFieldValues_ActiveDirectoryFields_FieldActiveDirectoryFieldId",
                table: "DirectoryTemplateFieldValues");

            migrationBuilder.DropTable(
                name: "AccessLevelActionAccessMapping");

            migrationBuilder.DropTable(
                name: "AccessLevelPermissionMap");

            migrationBuilder.DropTable(
                name: "PermissionDelegatePermissionMap");

            migrationBuilder.RenameColumn(
                name: "UserSettingsId",
                table: "UserSettings",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "PermissionMapId",
                table: "PermissionMap",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ActionAccessFlagId",
                table: "ObjectActionFlag",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ObjectAccessLevelId",
                table: "ObjectAccessLevel",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "FieldAccessLevelId",
                table: "FieldAccessLevel",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "EmailTemplateId",
                table: "EmailTemplates",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "EmailSettingsId",
                table: "EmailSettings",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "DirectoryTemplateId",
                table: "DirectoryTemplates",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "DirectoryTemplateGroupId",
                table: "DirectoryTemplateGroups",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "FieldActiveDirectoryFieldId",
                table: "DirectoryTemplateFieldValues",
                newName: "FieldId");

            migrationBuilder.RenameColumn(
                name: "DirectoryTemplateFieldValueId",
                table: "DirectoryTemplateFieldValues",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_DirectoryTemplateFieldValues_FieldActiveDirectoryFieldId",
                table: "DirectoryTemplateFieldValues",
                newName: "IX_DirectoryTemplateFieldValues_FieldId");

            migrationBuilder.RenameColumn(
                name: "AuthenticationSettingsId",
                table: "AuthenticationSettings",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "AppSettingsId",
                table: "AppSettings",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ADSettingsId",
                table: "ActiveDirectorySettings",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ActiveDirectoryFieldId",
                table: "ActiveDirectoryFields",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ObjectActionActionAccessFlagId",
                table: "ActionAccessMapping",
                newName: "ObjectActionId");

            migrationBuilder.RenameColumn(
                name: "ActionAccessMappingId",
                table: "ActionAccessMapping",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_ActionAccessMapping_ObjectActionActionAccessFlagId",
                table: "ActionAccessMapping",
                newName: "IX_ActionAccessMapping_ObjectActionId");

            migrationBuilder.RenameColumn(
                name: "AccessLevelId",
                table: "AccessLevels",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ObjectAccessMappingId",
                table: "AccessLevelObjectMapping",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ObjectMapObjectAccessMappingId",
                table: "AccessLevelObjectAccessMapping",
                newName: "ObjectMapId");

            migrationBuilder.RenameColumn(
                name: "AccessLevelsAccessLevelId",
                table: "AccessLevelObjectAccessMapping",
                newName: "AccessLevelsId");

            migrationBuilder.RenameIndex(
                name: "IX_AccessLevelObjectAccessMapping_ObjectMapObjectAccessMappingId",
                table: "AccessLevelObjectAccessMapping",
                newName: "IX_AccessLevelObjectAccessMapping_ObjectMapId");

            migrationBuilder.RenameColumn(
                name: "FieldAccessMappingId",
                table: "AccessLevelFieldMapping",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "FieldMapFieldAccessMappingId",
                table: "AccessLevelFieldAccessMapping",
                newName: "FieldMapId");

            migrationBuilder.RenameColumn(
                name: "AccessLevelsAccessLevelId",
                table: "AccessLevelFieldAccessMapping",
                newName: "AccessLevelsId");

            migrationBuilder.RenameIndex(
                name: "IX_AccessLevelFieldAccessMapping_FieldMapFieldAccessMappingId",
                table: "AccessLevelFieldAccessMapping",
                newName: "IX_AccessLevelFieldAccessMapping_FieldMapId");

            migrationBuilder.AddColumn<int>(
                name: "AccessLevelId",
                table: "ActionAccessMapping",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "AccessLevels",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AccessLevelPermissionMapping",
                columns: table => new
                {
                    AccessLevelsId = table.Column<int>(type: "INTEGER", nullable: false),
                    PermissionMapsId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessLevelPermissionMapping", x => new { x.AccessLevelsId, x.PermissionMapsId });
                    table.ForeignKey(
                        name: "FK_AccessLevelPermissionMapping_AccessLevels_AccessLevelsId",
                        column: x => x.AccessLevelsId,
                        principalTable: "AccessLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccessLevelPermissionMapping_PermissionMap_PermissionMapsId",
                        column: x => x.PermissionMapsId,
                        principalTable: "PermissionMap",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PermissionDelegatePermissionMapping",
                columns: table => new
                {
                    PermissionDelegatesId = table.Column<int>(type: "INTEGER", nullable: false),
                    PermissionsMapsId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionDelegatePermissionMapping", x => new { x.PermissionDelegatesId, x.PermissionsMapsId });
                    table.ForeignKey(
                        name: "FK_PermissionDelegatePermissionMapping_PermissionDelegate_PermissionDelegatesId",
                        column: x => x.PermissionDelegatesId,
                        principalTable: "PermissionDelegate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PermissionDelegatePermissionMapping_PermissionMap_PermissionsMapsId",
                        column: x => x.PermissionsMapsId,
                        principalTable: "PermissionMap",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AccessLevels",
                keyColumn: "Id",
                keyValue: 1,
                column: "DeletedAt",
                value: null);

            migrationBuilder.UpdateData(
                table: "AuthenticationSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "SessionTimeout",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_ActionAccessMapping_AccessLevelId",
                table: "ActionAccessMapping",
                column: "AccessLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessLevelPermissionMapping_PermissionMapsId",
                table: "AccessLevelPermissionMapping",
                column: "PermissionMapsId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionDelegatePermissionMapping_PermissionsMapsId",
                table: "PermissionDelegatePermissionMapping",
                column: "PermissionsMapsId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessLevelFieldAccessMapping_AccessLevelFieldMapping_FieldMapId",
                table: "AccessLevelFieldAccessMapping",
                column: "FieldMapId",
                principalTable: "AccessLevelFieldMapping",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AccessLevelFieldAccessMapping_AccessLevels_AccessLevelsId",
                table: "AccessLevelFieldAccessMapping",
                column: "AccessLevelsId",
                principalTable: "AccessLevels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AccessLevelObjectAccessMapping_AccessLevelObjectMapping_ObjectMapId",
                table: "AccessLevelObjectAccessMapping",
                column: "ObjectMapId",
                principalTable: "AccessLevelObjectMapping",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AccessLevelObjectAccessMapping_AccessLevels_AccessLevelsId",
                table: "AccessLevelObjectAccessMapping",
                column: "AccessLevelsId",
                principalTable: "AccessLevels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ActionAccessMapping_AccessLevels_AccessLevelId",
                table: "ActionAccessMapping",
                column: "AccessLevelId",
                principalTable: "AccessLevels",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ActionAccessMapping_ObjectActionFlag_ObjectActionId",
                table: "ActionAccessMapping",
                column: "ObjectActionId",
                principalTable: "ObjectActionFlag",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DirectoryTemplateFieldValues_ActiveDirectoryFields_FieldId",
                table: "DirectoryTemplateFieldValues",
                column: "FieldId",
                principalTable: "ActiveDirectoryFields",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessLevelFieldAccessMapping_AccessLevelFieldMapping_FieldMapId",
                table: "AccessLevelFieldAccessMapping");

            migrationBuilder.DropForeignKey(
                name: "FK_AccessLevelFieldAccessMapping_AccessLevels_AccessLevelsId",
                table: "AccessLevelFieldAccessMapping");

            migrationBuilder.DropForeignKey(
                name: "FK_AccessLevelObjectAccessMapping_AccessLevelObjectMapping_ObjectMapId",
                table: "AccessLevelObjectAccessMapping");

            migrationBuilder.DropForeignKey(
                name: "FK_AccessLevelObjectAccessMapping_AccessLevels_AccessLevelsId",
                table: "AccessLevelObjectAccessMapping");

            migrationBuilder.DropForeignKey(
                name: "FK_ActionAccessMapping_AccessLevels_AccessLevelId",
                table: "ActionAccessMapping");

            migrationBuilder.DropForeignKey(
                name: "FK_ActionAccessMapping_ObjectActionFlag_ObjectActionId",
                table: "ActionAccessMapping");

            migrationBuilder.DropForeignKey(
                name: "FK_DirectoryTemplateFieldValues_ActiveDirectoryFields_FieldId",
                table: "DirectoryTemplateFieldValues");

            migrationBuilder.DropTable(
                name: "AccessLevelPermissionMapping");

            migrationBuilder.DropTable(
                name: "PermissionDelegatePermissionMapping");

            migrationBuilder.DropIndex(
                name: "IX_ActionAccessMapping_AccessLevelId",
                table: "ActionAccessMapping");

            migrationBuilder.DropColumn(
                name: "AccessLevelId",
                table: "ActionAccessMapping");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "AccessLevels");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "UserSettings",
                newName: "UserSettingsId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "PermissionMap",
                newName: "PermissionMapId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ObjectActionFlag",
                newName: "ActionAccessFlagId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ObjectAccessLevel",
                newName: "ObjectAccessLevelId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "FieldAccessLevel",
                newName: "FieldAccessLevelId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "EmailTemplates",
                newName: "EmailTemplateId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "EmailSettings",
                newName: "EmailSettingsId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "DirectoryTemplates",
                newName: "DirectoryTemplateId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "DirectoryTemplateGroups",
                newName: "DirectoryTemplateGroupId");

            migrationBuilder.RenameColumn(
                name: "FieldId",
                table: "DirectoryTemplateFieldValues",
                newName: "FieldActiveDirectoryFieldId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "DirectoryTemplateFieldValues",
                newName: "DirectoryTemplateFieldValueId");

            migrationBuilder.RenameIndex(
                name: "IX_DirectoryTemplateFieldValues_FieldId",
                table: "DirectoryTemplateFieldValues",
                newName: "IX_DirectoryTemplateFieldValues_FieldActiveDirectoryFieldId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "AuthenticationSettings",
                newName: "AuthenticationSettingsId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "AppSettings",
                newName: "AppSettingsId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ActiveDirectorySettings",
                newName: "ADSettingsId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ActiveDirectoryFields",
                newName: "ActiveDirectoryFieldId");

            migrationBuilder.RenameColumn(
                name: "ObjectActionId",
                table: "ActionAccessMapping",
                newName: "ObjectActionActionAccessFlagId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ActionAccessMapping",
                newName: "ActionAccessMappingId");

            migrationBuilder.RenameIndex(
                name: "IX_ActionAccessMapping_ObjectActionId",
                table: "ActionAccessMapping",
                newName: "IX_ActionAccessMapping_ObjectActionActionAccessFlagId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "AccessLevels",
                newName: "AccessLevelId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "AccessLevelObjectMapping",
                newName: "ObjectAccessMappingId");

            migrationBuilder.RenameColumn(
                name: "ObjectMapId",
                table: "AccessLevelObjectAccessMapping",
                newName: "ObjectMapObjectAccessMappingId");

            migrationBuilder.RenameColumn(
                name: "AccessLevelsId",
                table: "AccessLevelObjectAccessMapping",
                newName: "AccessLevelsAccessLevelId");

            migrationBuilder.RenameIndex(
                name: "IX_AccessLevelObjectAccessMapping_ObjectMapId",
                table: "AccessLevelObjectAccessMapping",
                newName: "IX_AccessLevelObjectAccessMapping_ObjectMapObjectAccessMappingId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "AccessLevelFieldMapping",
                newName: "FieldAccessMappingId");

            migrationBuilder.RenameColumn(
                name: "FieldMapId",
                table: "AccessLevelFieldAccessMapping",
                newName: "FieldMapFieldAccessMappingId");

            migrationBuilder.RenameColumn(
                name: "AccessLevelsId",
                table: "AccessLevelFieldAccessMapping",
                newName: "AccessLevelsAccessLevelId");

            migrationBuilder.RenameIndex(
                name: "IX_AccessLevelFieldAccessMapping_FieldMapId",
                table: "AccessLevelFieldAccessMapping",
                newName: "IX_AccessLevelFieldAccessMapping_FieldMapFieldAccessMappingId");

            migrationBuilder.CreateTable(
                name: "AccessLevelActionAccessMapping",
                columns: table => new
                {
                    AccessLevelsAccessLevelId = table.Column<int>(type: "INTEGER", nullable: false),
                    ActionMapActionAccessMappingId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessLevelActionAccessMapping", x => new { x.AccessLevelsAccessLevelId, x.ActionMapActionAccessMappingId });
                    table.ForeignKey(
                        name: "FK_AccessLevelActionAccessMapping_AccessLevels_AccessLevelsAccessLevelId",
                        column: x => x.AccessLevelsAccessLevelId,
                        principalTable: "AccessLevels",
                        principalColumn: "AccessLevelId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccessLevelActionAccessMapping_ActionAccessMapping_ActionMapActionAccessMappingId",
                        column: x => x.ActionMapActionAccessMappingId,
                        principalTable: "ActionAccessMapping",
                        principalColumn: "ActionAccessMappingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccessLevelPermissionMap",
                columns: table => new
                {
                    AccessLevelsAccessLevelId = table.Column<int>(type: "INTEGER", nullable: false),
                    PermissionMapsPermissionMapId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessLevelPermissionMap", x => new { x.AccessLevelsAccessLevelId, x.PermissionMapsPermissionMapId });
                    table.ForeignKey(
                        name: "FK_AccessLevelPermissionMap_AccessLevels_AccessLevelsAccessLevelId",
                        column: x => x.AccessLevelsAccessLevelId,
                        principalTable: "AccessLevels",
                        principalColumn: "AccessLevelId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccessLevelPermissionMap_PermissionMap_PermissionMapsPermissionMapId",
                        column: x => x.PermissionMapsPermissionMapId,
                        principalTable: "PermissionMap",
                        principalColumn: "PermissionMapId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PermissionDelegatePermissionMap",
                columns: table => new
                {
                    PermissionDelegatesId = table.Column<int>(type: "INTEGER", nullable: false),
                    PermissionsMapsPermissionMapId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionDelegatePermissionMap", x => new { x.PermissionDelegatesId, x.PermissionsMapsPermissionMapId });
                    table.ForeignKey(
                        name: "FK_PermissionDelegatePermissionMap_PermissionDelegate_PermissionDelegatesId",
                        column: x => x.PermissionDelegatesId,
                        principalTable: "PermissionDelegate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PermissionDelegatePermissionMap_PermissionMap_PermissionsMapsPermissionMapId",
                        column: x => x.PermissionsMapsPermissionMapId,
                        principalTable: "PermissionMap",
                        principalColumn: "PermissionMapId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AuthenticationSettings",
                keyColumn: "AuthenticationSettingsId",
                keyValue: 1,
                column: "SessionTimeout",
                value: 900000);

            migrationBuilder.CreateIndex(
                name: "IX_AccessLevelActionAccessMapping_ActionMapActionAccessMappingId",
                table: "AccessLevelActionAccessMapping",
                column: "ActionMapActionAccessMappingId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessLevelPermissionMap_PermissionMapsPermissionMapId",
                table: "AccessLevelPermissionMap",
                column: "PermissionMapsPermissionMapId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionDelegatePermissionMap_PermissionsMapsPermissionMapId",
                table: "PermissionDelegatePermissionMap",
                column: "PermissionsMapsPermissionMapId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessLevelFieldAccessMapping_AccessLevelFieldMapping_FieldMapFieldAccessMappingId",
                table: "AccessLevelFieldAccessMapping",
                column: "FieldMapFieldAccessMappingId",
                principalTable: "AccessLevelFieldMapping",
                principalColumn: "FieldAccessMappingId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AccessLevelFieldAccessMapping_AccessLevels_AccessLevelsAccessLevelId",
                table: "AccessLevelFieldAccessMapping",
                column: "AccessLevelsAccessLevelId",
                principalTable: "AccessLevels",
                principalColumn: "AccessLevelId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AccessLevelObjectAccessMapping_AccessLevelObjectMapping_ObjectMapObjectAccessMappingId",
                table: "AccessLevelObjectAccessMapping",
                column: "ObjectMapObjectAccessMappingId",
                principalTable: "AccessLevelObjectMapping",
                principalColumn: "ObjectAccessMappingId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AccessLevelObjectAccessMapping_AccessLevels_AccessLevelsAccessLevelId",
                table: "AccessLevelObjectAccessMapping",
                column: "AccessLevelsAccessLevelId",
                principalTable: "AccessLevels",
                principalColumn: "AccessLevelId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ActionAccessMapping_ObjectActionFlag_ObjectActionActionAccessFlagId",
                table: "ActionAccessMapping",
                column: "ObjectActionActionAccessFlagId",
                principalTable: "ObjectActionFlag",
                principalColumn: "ActionAccessFlagId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DirectoryTemplateFieldValues_ActiveDirectoryFields_FieldActiveDirectoryFieldId",
                table: "DirectoryTemplateFieldValues",
                column: "FieldActiveDirectoryFieldId",
                principalTable: "ActiveDirectoryFields",
                principalColumn: "ActiveDirectoryFieldId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
