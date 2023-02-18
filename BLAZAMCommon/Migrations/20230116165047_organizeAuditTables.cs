using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Common.Migrations
{
    /// <inheritdoc />
    public partial class organizeAuditTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserAuditLog",
                table: "UserAuditLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SystemAuditLog",
                table: "SystemAuditLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SettingsAuditLog",
                table: "SettingsAuditLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RequestAuditLog",
                table: "RequestAuditLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PermissionsAuditLog",
                table: "PermissionsAuditLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OUAuditLog",
                table: "OUAuditLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LogonAuditLog",
                table: "LogonAuditLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GroupAuditLog",
                table: "GroupAuditLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ComputerAuditLog",
                table: "ComputerAuditLog");

            migrationBuilder.RenameTable(
                name: "UserAuditLog",
                newName: "Audit.UserAuditLog");

            migrationBuilder.RenameTable(
                name: "SystemAuditLog",
                newName: "Audit.SystemAuditLog");

            migrationBuilder.RenameTable(
                name: "SettingsAuditLog",
                newName: "Audit.SettingsAuditLog");

            migrationBuilder.RenameTable(
                name: "RequestAuditLog",
                newName: "Audit.RequestAuditLog");

            migrationBuilder.RenameTable(
                name: "PermissionsAuditLog",
                newName: "Audit.PermissionsAuditLog");

            migrationBuilder.RenameTable(
                name: "OUAuditLog",
                newName: "Audit.OUAuditLog");

            migrationBuilder.RenameTable(
                name: "LogonAuditLog",
                newName: "Audit.LogonAuditLog");

            migrationBuilder.RenameTable(
                name: "GroupAuditLog",
                newName: "Audit.GroupAuditLog");

            migrationBuilder.RenameTable(
                name: "ComputerAuditLog",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.RenameTable(
                name: "Audit.UserAuditLog",
                newName: "UserAuditLog");

            migrationBuilder.RenameTable(
                name: "Audit.SystemAuditLog",
                newName: "SystemAuditLog");

            migrationBuilder.RenameTable(
                name: "Audit.SettingsAuditLog",
                newName: "SettingsAuditLog");

            migrationBuilder.RenameTable(
                name: "Audit.RequestAuditLog",
                newName: "RequestAuditLog");

            migrationBuilder.RenameTable(
                name: "Audit.PermissionsAuditLog",
                newName: "PermissionsAuditLog");

            migrationBuilder.RenameTable(
                name: "Audit.OUAuditLog",
                newName: "OUAuditLog");

            migrationBuilder.RenameTable(
                name: "Audit.LogonAuditLog",
                newName: "LogonAuditLog");

            migrationBuilder.RenameTable(
                name: "Audit.GroupAuditLog",
                newName: "GroupAuditLog");

            migrationBuilder.RenameTable(
                name: "Audit.ComputerAuditLog",
                newName: "ComputerAuditLog");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserAuditLog",
                table: "UserAuditLog",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SystemAuditLog",
                table: "SystemAuditLog",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SettingsAuditLog",
                table: "SettingsAuditLog",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RequestAuditLog",
                table: "RequestAuditLog",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PermissionsAuditLog",
                table: "PermissionsAuditLog",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OUAuditLog",
                table: "OUAuditLog",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LogonAuditLog",
                table: "LogonAuditLog",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GroupAuditLog",
                table: "GroupAuditLog",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ComputerAuditLog",
                table: "ComputerAuditLog",
                column: "Id");
        }
    }
}
