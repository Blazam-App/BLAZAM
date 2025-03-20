using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Database.Migrations.Sql
{
    /// <inheritdoc />
    public partial class Add_GoogleAuth_MFAType_Sql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MFAType",
                table: "AuthenticationSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "ActiveDirectoryFields",
                columns: new[] { "Id", "DisplayName", "FieldName", "FieldType" },
                values: new object[] { 38, "Group Type and Scope", "groupType", 2 });

            migrationBuilder.UpdateData(
                table: "AuthenticationSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "MFAType",
                value: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DropColumn(
                name: "MFAType",
                table: "AuthenticationSettings");
        }
    }
}
