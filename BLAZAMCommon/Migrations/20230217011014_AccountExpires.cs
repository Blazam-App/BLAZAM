using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Common.Migrations
{
    /// <inheritdoc />
    public partial class AccountExpires : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ActiveDirectoryFields",
                columns: new[] { "ActiveDirectoryFieldId", "DisplayName", "FieldName" },
                values: new object[] { 33, "Account Expiration", "accountExpires" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ActiveDirectoryFields",
                keyColumn: "ActiveDirectoryFieldId",
                keyValue: 33);
        }
    }
}
