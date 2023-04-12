using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Common.Migrations.Sql
{
    /// <inheritdoc />
    public partial class FixCityFieldNameSql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 10,
                column: "FieldName",
                value: "l");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ActiveDirectoryFields",
                keyColumn: "Id",
                keyValue: 10,
                column: "FieldName",
                value: "city");
        }
    }
}
