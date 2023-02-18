using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Common.Migrations
{
    /// <inheritdoc />
    public partial class organizeAuditTables2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Audit.UserAuditLog",
                table: "Audit.UserAuditLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Audit.SystemAuditLog",
                table: "Audit.SystemAuditLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Audit.SettingsAuditLog",
                table: "Audit.SettingsAuditLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Audit.RequestAuditLog",
                table: "Audit.RequestAuditLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Audit.PermissionsAuditLog",
                table: "Audit.PermissionsAuditLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Audit.OUAuditLog",
                table: "Audit.OUAuditLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Audit.LogonAuditLog",
                table: "Audit.LogonAuditLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Audit.GroupAuditLog",
                table: "Audit.GroupAuditLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Audit.ComputerAuditLog",
                table: "Audit.ComputerAuditLog");

            migrationBuilder.EnsureSchema(
                name: "Audit");

            migrationBuilder.RenameTable(
                name: "Audit.UserAuditLog",
                newName: "UserAuditLog",
                newSchema: "Audit");

            migrationBuilder.RenameTable(
                name: "Audit.SystemAuditLog",
                newName: "SystemAuditLog",
                newSchema: "Audit");

            migrationBuilder.RenameTable(
                name: "Audit.SettingsAuditLog",
                newName: "SettingsAuditLog",
                newSchema: "Audit");

            migrationBuilder.RenameTable(
                name: "Audit.RequestAuditLog",
                newName: "RequestAuditLog",
                newSchema: "Audit");

            migrationBuilder.RenameTable(
                name: "Audit.PermissionsAuditLog",
                newName: "PermissionsAuditLog",
                newSchema: "Audit");

            migrationBuilder.RenameTable(
                name: "Audit.OUAuditLog",
                newName: "OUAuditLog",
                newSchema: "Audit");

            migrationBuilder.RenameTable(
                name: "Audit.LogonAuditLog",
                newName: "LogonAuditLog",
                newSchema: "Audit");

            migrationBuilder.RenameTable(
                name: "Audit.GroupAuditLog",
                newName: "GroupAuditLog",
                newSchema: "Audit");

            migrationBuilder.RenameTable(
                name: "Audit.ComputerAuditLog",
                newName: "ComputerAuditLog",
                newSchema: "Audit");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserAuditLog",
                schema: "Audit",
                table: "UserAuditLog",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SystemAuditLog",
                schema: "Audit",
                table: "SystemAuditLog",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SettingsAuditLog",
                schema: "Audit",
                table: "SettingsAuditLog",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RequestAuditLog",
                schema: "Audit",
                table: "RequestAuditLog",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PermissionsAuditLog",
                schema: "Audit",
                table: "PermissionsAuditLog",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OUAuditLog",
                schema: "Audit",
                table: "OUAuditLog",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LogonAuditLog",
                schema: "Audit",
                table: "LogonAuditLog",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GroupAuditLog",
                schema: "Audit",
                table: "GroupAuditLog",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ComputerAuditLog",
                schema: "Audit",
                table: "ComputerAuditLog",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserAuditLog",
                schema: "Audit",
                table: "UserAuditLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SystemAuditLog",
                schema: "Audit",
                table: "SystemAuditLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SettingsAuditLog",
                schema: "Audit",
                table: "SettingsAuditLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RequestAuditLog",
                schema: "Audit",
                table: "RequestAuditLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PermissionsAuditLog",
                schema: "Audit",
                table: "PermissionsAuditLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OUAuditLog",
                schema: "Audit",
                table: "OUAuditLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LogonAuditLog",
                schema: "Audit",
                table: "LogonAuditLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GroupAuditLog",
                schema: "Audit",
                table: "GroupAuditLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ComputerAuditLog",
                schema: "Audit",
                table: "ComputerAuditLog");

            migrationBuilder.RenameTable(
                name: "UserAuditLog",
                schema: "Audit",
                newName: "Audit.UserAuditLog");

            migrationBuilder.RenameTable(
                name: "SystemAuditLog",
                schema: "Audit",
                newName: "Audit.SystemAuditLog");

            migrationBuilder.RenameTable(
                name: "SettingsAuditLog",
                schema: "Audit",
                newName: "Audit.SettingsAuditLog");

            migrationBuilder.RenameTable(
                name: "RequestAuditLog",
                schema: "Audit",
                newName: "Audit.RequestAuditLog");

            migrationBuilder.RenameTable(
                name: "PermissionsAuditLog",
                schema: "Audit",
                newName: "Audit.PermissionsAuditLog");

            migrationBuilder.RenameTable(
                name: "OUAuditLog",
                schema: "Audit",
                newName: "Audit.OUAuditLog");

            migrationBuilder.RenameTable(
                name: "LogonAuditLog",
                schema: "Audit",
                newName: "Audit.LogonAuditLog");

            migrationBuilder.RenameTable(
                name: "GroupAuditLog",
                schema: "Audit",
                newName: "Audit.GroupAuditLog");

            migrationBuilder.RenameTable(
                name: "ComputerAuditLog",
                schema: "Audit",
                newName: "Audit.ComputerAuditLog");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Audit.UserAuditLog",
                table: "Audit.UserAuditLog",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Audit.SystemAuditLog",
                table: "Audit.SystemAuditLog",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Audit.SettingsAuditLog",
                table: "Audit.SettingsAuditLog",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Audit.RequestAuditLog",
                table: "Audit.RequestAuditLog",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Audit.PermissionsAuditLog",
                table: "Audit.PermissionsAuditLog",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Audit.OUAuditLog",
                table: "Audit.OUAuditLog",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Audit.LogonAuditLog",
                table: "Audit.LogonAuditLog",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Audit.GroupAuditLog",
                table: "Audit.GroupAuditLog",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Audit.ComputerAuditLog",
                table: "Audit.ComputerAuditLog",
                column: "Id");
        }
    }
}
