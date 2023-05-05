using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Common.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class CustomADFieldsSqlite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Action",
                table: "ObjectActionFlag",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FieldType",
                table: "ActiveDirectoryFields",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CustomActiveDirectoryFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FieldName = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    FieldType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomActiveDirectoryFields", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ActiveDirectoryFieldObjectMappings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ObjectType = table.Column<int>(type: "INTEGER", nullable: false),
                    ActiveDirectoryFieldId = table.Column<int>(type: "INTEGER", nullable: false),
                    CustomActiveDirectoryFieldId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveDirectoryFieldObjectMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActiveDirectoryFieldObjectMappings_CustomActiveDirectoryFields_CustomActiveDirectoryFieldId",
                        column: x => x.CustomActiveDirectoryFieldId,
                        principalTable: "CustomActiveDirectoryFields",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 1,
                column: "FieldType",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 2,
                column: "FieldType",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 3,
                column: "FieldType",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 4,
                column: "FieldType",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 5,
                column: "FieldType",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 6,
                column: "FieldType",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 7,
                column: "FieldType",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 8,
                column: "FieldType",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 9,
                column: "FieldType",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 10,
                column: "FieldType",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 11,
                column: "FieldType",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 12,
                column: "FieldType",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 13,
                column: "FieldType",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 14,
                column: "FieldType",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 15,
                column: "FieldType",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 16,
                column: "FieldType",
                value: 2);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 17,
                column: "FieldType",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 18,
                column: "FieldType",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 19,
                column: "FieldType",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 20,
                column: "FieldType",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 21,
                column: "FieldType",
                value: 4);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 22,
                column: "FieldType",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 23,
                column: "FieldType",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 24,
                column: "FieldType",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 25,
                column: "FieldType",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 26,
                column: "FieldType",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 27,
                column: "FieldType",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 28,
                column: "FieldType",
                value: 3);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 29,
                column: "FieldType",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 30,
                column: "FieldType",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 31,
                column: "FieldType",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 32,
                column: "FieldType",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 33,
                column: "FieldType",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 34,
                column: "FieldType",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ObjectActionFlag",
                keyColumn: "Id",
                keyValue: 1,
                column: "Action",
                value: 4);

            migrationBuilder.UpdateData(
                table: "ObjectActionFlag",
                keyColumn: "Id",
                keyValue: 2,
                column: "Action",
                value: 3);

            migrationBuilder.UpdateData(
                table: "ObjectActionFlag",
                keyColumn: "Id",
                keyValue: 3,
                column: "Action",
                value: 8);

            migrationBuilder.UpdateData(
                table: "ObjectActionFlag",
                keyColumn: "Id",
                keyValue: 4,
                column: "Action",
                value: 5);

            migrationBuilder.UpdateData(
                table: "ObjectActionFlag",
                keyColumn: "Id",
                keyValue: 5,
                column: "Action",
                value: 6);

            migrationBuilder.UpdateData(
                table: "ObjectActionFlag",
                keyColumn: "Id",
                keyValue: 6,
                column: "Action",
                value: 7);

            migrationBuilder.UpdateData(
                table: "ObjectActionFlag",
                keyColumn: "Id",
                keyValue: 7,
                column: "Action",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ObjectActionFlag",
                keyColumn: "Id",
                keyValue: 8,
                column: "Action",
                value: 2);

            migrationBuilder.UpdateData(
                table: "ObjectActionFlag",
                keyColumn: "Id",
                keyValue: 9,
                column: "Action",
                value: 1);

            migrationBuilder.CreateIndex(
                name: "IX_ActiveDirectoryFieldObjectMappings_CustomActiveDirectoryFieldId",
                table: "ActiveDirectoryFieldObjectMappings",
                column: "CustomActiveDirectoryFieldId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActiveDirectoryFieldObjectMappings");

            migrationBuilder.DropTable(
                name: "CustomActiveDirectoryFields");

            migrationBuilder.DropColumn(
                name: "Action",
                table: "ObjectActionFlag");

            migrationBuilder.DropColumn(
                name: "FieldType",
                table: "ActiveDirectoryFields");
        }
    }
}
