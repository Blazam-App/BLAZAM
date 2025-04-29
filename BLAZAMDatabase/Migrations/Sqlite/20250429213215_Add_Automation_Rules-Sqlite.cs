using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BLAZAM.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class Add_Automation_RulesSqlite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PropertyName",
                table: "ActiveDirectoryFields",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "AutomationRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    LastTriggered = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastExcecuted = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    StopOnThisRule = table.Column<bool>(type: "INTEGER", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ScheduleInterval = table.Column<int>(type: "INTEGER", nullable: true),
                    ScheduledRunTime = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    IntervalCount = table.Column<int>(type: "INTEGER", nullable: true),
                    Trigger = table.Column<int>(type: "INTEGER", nullable: false),
                    ActiveDirectoryObjectType = table.Column<int>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AutomationRuleActions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ActionType = table.Column<int>(type: "INTEGER", nullable: false),
                    ActiveDirectoryObjectAction = table.Column<int>(type: "INTEGER", nullable: false),
                    Data = table.Column<string>(type: "TEXT", nullable: true),
                    ActionGuid = table.Column<Guid>(type: "TEXT", nullable: false),
                    AutomationRuleId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationRuleActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutomationRuleActions_AutomationRules_AutomationRuleId",
                        column: x => x.AutomationRuleId,
                        principalTable: "AutomationRules",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AutomationRuleOrFilter",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AutomationRuleId = table.Column<int>(type: "INTEGER", nullable: false),
                    FilterGuid = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationRuleOrFilter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutomationRuleOrFilter_AutomationRules_AutomationRuleId",
                        column: x => x.AutomationRuleId,
                        principalTable: "AutomationRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AutomationRuleFieldValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Value = table.Column<string>(type: "TEXT", nullable: true),
                    AutomationRuleActionId = table.Column<int>(type: "INTEGER", nullable: false),
                    FieldId = table.Column<int>(type: "INTEGER", nullable: true),
                    CustomFieldId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationRuleFieldValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutomationRuleFieldValues_ActiveDirectoryFields_FieldId",
                        column: x => x.FieldId,
                        principalTable: "ActiveDirectoryFields",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AutomationRuleFieldValues_AutomationRuleActions_AutomationRuleActionId",
                        column: x => x.AutomationRuleActionId,
                        principalTable: "AutomationRuleActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AutomationRuleFieldValues_CustomActiveDirectoryFields_CustomFieldId",
                        column: x => x.CustomFieldId,
                        principalTable: "CustomActiveDirectoryFields",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AutomationRuleGroupSids",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GroupSid = table.Column<string>(type: "TEXT", nullable: false),
                    Assigned = table.Column<bool>(type: "INTEGER", nullable: false),
                    AutomationRuleActionId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationRuleGroupSids", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutomationRuleGroupSids_AutomationRuleActions_AutomationRuleActionId",
                        column: x => x.AutomationRuleActionId,
                        principalTable: "AutomationRuleActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AutomationRuleAndFilters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrFilterId = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true),
                    Operator = table.Column<int>(type: "INTEGER", nullable: false),
                    Negate = table.Column<bool>(type: "INTEGER", nullable: false),
                    TimeFrame = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    FilterGuid = table.Column<Guid>(type: "TEXT", nullable: false),
                    FieldId = table.Column<int>(type: "INTEGER", nullable: true),
                    CustomFieldId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationRuleAndFilters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutomationRuleAndFilters_ActiveDirectoryFields_FieldId",
                        column: x => x.FieldId,
                        principalTable: "ActiveDirectoryFields",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AutomationRuleAndFilters_AutomationRuleOrFilter_OrFilterId",
                        column: x => x.OrFilterId,
                        principalTable: "AutomationRuleOrFilter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AutomationRuleAndFilters_CustomActiveDirectoryFields_CustomFieldId",
                        column: x => x.CustomFieldId,
                        principalTable: "CustomActiveDirectoryFields",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 1,
                column: "PropertyName",
                value: "Sn");

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 2,
                column: "PropertyName",
                value: "GivenName");

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 3,
                column: "PropertyName",
                value: "PhysicalDeliveryOfficeName");

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "DisplayName", "PropertyName" },
                values: new object[] { "Employee Id", "EmployeeId" });

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 5,
                column: "PropertyName",
                value: "HomeDirectory");

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 6,
                column: "PropertyName",
                value: "ScriptPath");

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 7,
                column: "PropertyName",
                value: "ProfilePath");

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 8,
                column: "PropertyName",
                value: "HomePhone");

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 9,
                column: "PropertyName",
                value: "StreetAddress");

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 10,
                column: "PropertyName",
                value: "City");

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 11,
                column: "PropertyName",
                value: "State");

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 12,
                column: "PropertyName",
                value: "Zip");

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 13,
                column: "PropertyName",
                value: "Site");

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 14,
                column: "PropertyName",
                value: "Name");

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 15,
                column: "PropertyName",
                value: "SAMAccountName");

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 16,
                column: "PropertyName",
                value: "SID");

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 17,
                column: "PropertyName",
                value: "Email");

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 18,
                column: "PropertyName",
                value: "Description");

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 19,
                column: "PropertyName",
                value: "DisplayName");

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 20,
                column: "PropertyName",
                value: "DN");

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 21,
                column: "PropertyName",
                value: "MemberOf");

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 22,
                column: "PropertyName",
                value: "Company");

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 23,
                column: "PropertyName",
                value: "Title");

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 24,
                column: "PropertyName",
                value: "UserPrincipalName");

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 25,
                column: "PropertyName",
                value: "TelephoneNumber");

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 26,
                column: "PropertyName",
                value: "POBox");

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 27,
                column: "PropertyName",
                value: "CanonicalName");

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 28,
                column: "PropertyName",
                value: "HomeDrive");

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 29,
                column: "PropertyName",
                value: "Department");

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 30,
                column: "PropertyName",
                value: "MiddleName");

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 31,
                column: "PropertyName",
                value: "Pager");

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 32,
                column: "PropertyName",
                value: "OS");

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "FieldType", "PropertyName" },
                values: new object[] { 5, "ExpireTime" });

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 34,
                column: "PropertyName",
                value: "Manager");

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 35,
                column: "PropertyName",
                value: "ThumbnailPhoto");

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 36,
                column: "PropertyName",
                value: "LogOnTo");

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 37,
                column: "PropertyName",
                value: "LogonHours");

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "DisplayName", "PropertyName" },
                values: new object[] { "Group Type", "GroupType" });

            migrationBuilder.InsertData(
                table: "ActiveDirectoryFields",
                columns: new[] { "Id", "DisplayName", "FieldName", "FieldType", "PropertyName" },
                values: new object[,]
                {
                    { 39, "Group Scope", "groupType", 2, "GroupScope" },
                    { 40, "Enabled", "userAccountControl", 6, "Enabled" },
                    { 41, "Locked Out", "lockoutTime", 6, "LockedOut" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRuleActions_AutomationRuleId",
                table: "AutomationRuleActions",
                column: "AutomationRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRuleAndFilters_CustomFieldId",
                table: "AutomationRuleAndFilters",
                column: "CustomFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRuleAndFilters_FieldId",
                table: "AutomationRuleAndFilters",
                column: "FieldId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRuleAndFilters_OrFilterId",
                table: "AutomationRuleAndFilters",
                column: "OrFilterId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRuleFieldValues_AutomationRuleActionId",
                table: "AutomationRuleFieldValues",
                column: "AutomationRuleActionId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRuleFieldValues_CustomFieldId",
                table: "AutomationRuleFieldValues",
                column: "CustomFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRuleFieldValues_FieldId",
                table: "AutomationRuleFieldValues",
                column: "FieldId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRuleGroupSids_AutomationRuleActionId",
                table: "AutomationRuleGroupSids",
                column: "AutomationRuleActionId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRuleOrFilter_AutomationRuleId",
                table: "AutomationRuleOrFilter",
                column: "AutomationRuleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AutomationRuleAndFilters");

            migrationBuilder.DropTable(
                name: "AutomationRuleFieldValues");

            migrationBuilder.DropTable(
                name: "AutomationRuleGroupSids");

            migrationBuilder.DropTable(
                name: "AutomationRuleOrFilter");

            migrationBuilder.DropTable(
                name: "AutomationRuleActions");

            migrationBuilder.DropTable(
                name: "AutomationRules");

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DropColumn(
                name: "PropertyName",
                table: "ActiveDirectoryFields");

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 4,
                column: "DisplayName",
                value: "Employee ID");

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 33,
                column: "FieldType",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 38,
                column: "DisplayName",
                value: "Group Type and Scope");
        }
    }
}
