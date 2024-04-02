using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Database.Migrations.Sql
{
    /// <inheritdoc />
    public partial class Add_Duo_Config_OptionsSql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DuoEnabled",
                table: "AuthenticationSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "DuoUnreachableBehavior",
                table: "AuthenticationSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "AuthenticationSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DuoEnabled", "DuoUnreachableBehavior" },
                values: new object[] { false, 0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DuoEnabled",
                table: "AuthenticationSettings");

            migrationBuilder.DropColumn(
                name: "DuoUnreachableBehavior",
                table: "AuthenticationSettings");
        }
    }
}
