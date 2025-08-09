using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class Add_Email_Audit_Sqlite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MessageGuid = table.Column<string>(type: "TEXT", nullable: true),
                    From = table.Column<string>(type: "TEXT", nullable: true),
                    To = table.Column<string>(type: "TEXT", nullable: true),
                    Cc = table.Column<string>(type: "TEXT", nullable: true),
                    Bcc = table.Column<string>(type: "TEXT", nullable: true),
                    Subject = table.Column<string>(type: "TEXT", nullable: true),
                    HtmlBody = table.Column<string>(type: "TEXT", nullable: true),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastAttemptTimestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Retries = table.Column<int>(type: "INTEGER", nullable: false),
                    ReadTimestamp = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ServerResponse = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailAuditLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FailedADLogonEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WorkstationName = table.Column<string>(type: "TEXT", nullable: true),
                    WorkstationIp = table.Column<string>(type: "TEXT", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Sid = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FailedADLogonEvents", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailAuditLog");

            migrationBuilder.DropTable(
                name: "FailedADLogonEvents");
        }
    }
}
