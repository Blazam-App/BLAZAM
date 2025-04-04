using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BLAZAM.Database.Migrations.Sql
{
    /// <inheritdoc />
    public partial class Add_Automation_Rules_Sql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PropertyName",
                table: "CustomActiveDirectoryFields",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PropertyName",
                table: "ActiveDirectoryFields",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AutomationRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastTriggered = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastExcecuted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    StopOnThisRule = table.Column<bool>(type: "bit", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ScheduledRunTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    Trigger = table.Column<int>(type: "int", nullable: false),
                    ActiveDirectoryObjectType = table.Column<int>(type: "int", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AutomationRuleActions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActionType = table.Column<int>(type: "int", nullable: false),
                    ActiveDirectoryObjectAction = table.Column<int>(type: "int", nullable: false),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActionGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AutomationRuleId = table.Column<int>(type: "int", nullable: true)
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
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AutomationRuleId = table.Column<int>(type: "int", nullable: false),
                    FilterGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FieldId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AutomationRuleActionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationRuleFieldValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutomationRuleFieldValues_ActiveDirectoryFields_FieldId",
                        column: x => x.FieldId,
                        principalTable: "ActiveDirectoryFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AutomationRuleFieldValues_AutomationRuleActions_AutomationRuleActionId",
                        column: x => x.AutomationRuleActionId,
                        principalTable: "AutomationRuleActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AutomationRuleGroupSids",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupSid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Assigned = table.Column<bool>(type: "bit", nullable: false),
                    AutomationRuleActionId = table.Column<int>(type: "int", nullable: false)
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
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrFilterId = table.Column<int>(type: "int", nullable: false),
                    FieldId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Operator = table.Column<int>(type: "int", nullable: false),
                    Negate = table.Column<bool>(type: "bit", nullable: false),
                    TimeFrame = table.Column<TimeSpan>(type: "time", nullable: true),
                    FilterGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationRuleAndFilters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutomationRuleAndFilters_ActiveDirectoryFields_FieldId",
                        column: x => x.FieldId,
                        principalTable: "ActiveDirectoryFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AutomationRuleAndFilters_AutomationRuleOrFilter_OrFilterId",
                        column: x => x.OrFilterId,
                        principalTable: "AutomationRuleOrFilter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 1,
                column: "PropertyName",
                value: null);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 2,
                column: "PropertyName",
                value: null);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 3,
                column: "PropertyName",
                value: null);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "DisplayName", "PropertyName" },
                values: new object[] { "Employee Id", null });

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 5,
                column: "PropertyName",
                value: null);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 6,
                column: "PropertyName",
                value: null);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 7,
                column: "PropertyName",
                value: null);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 8,
                column: "PropertyName",
                value: null);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 9,
                column: "PropertyName",
                value: null);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 10,
                column: "PropertyName",
                value: null);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 11,
                column: "PropertyName",
                value: null);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 12,
                column: "PropertyName",
                value: null);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 13,
                column: "PropertyName",
                value: null);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 14,
                column: "PropertyName",
                value: null);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 15,
                column: "PropertyName",
                value: null);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 16,
                column: "PropertyName",
                value: null);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 17,
                column: "PropertyName",
                value: null);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 18,
                column: "PropertyName",
                value: null);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 19,
                column: "PropertyName",
                value: null);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 20,
                column: "PropertyName",
                value: null);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 21,
                column: "PropertyName",
                value: null);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 22,
                column: "PropertyName",
                value: null);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 23,
                column: "PropertyName",
                value: null);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 24,
                column: "PropertyName",
                value: null);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 25,
                column: "PropertyName",
                value: null);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 26,
                column: "PropertyName",
                value: null);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 27,
                column: "PropertyName",
                value: null);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 28,
                column: "PropertyName",
                value: null);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 29,
                column: "PropertyName",
                value: null);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 30,
                column: "PropertyName",
                value: null);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 31,
                column: "PropertyName",
                value: null);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 32,
                column: "PropertyName",
                value: null);

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
                value: null);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 35,
                column: "PropertyName",
                value: null);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 36,
                column: "PropertyName",
                value: null);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 37,
                column: "PropertyName",
                value: null);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 38,
                column: "PropertyName",
                value: null);

            migrationBuilder.InsertData(
                table: "ActiveDirectoryFields",
                columns: new[] { "Id", "DisplayName", "FieldName", "FieldType", "PropertyName" },
                values: new object[,]
                {
                    { 39, "Enabled", "userAccountControl", 6, "Enabled" },
                    { 40, "Locked_ Out", "lockoutTime", 5, "LockedOut" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRuleActions_AutomationRuleId",
                table: "AutomationRuleActions",
                column: "AutomationRuleId");

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

            migrationBuilder.DropColumn(
                name: "PropertyName",
                table: "CustomActiveDirectoryFields");

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
        }
    }
}
