using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Database.Migrations.Sql
{
    /// <inheritdoc />
    public partial class Add_Email_Audit_Sql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageGuid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    From = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    To = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bcc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HtmlBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    LastAttemptTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Retries = table.Column<int>(type: "int", nullable: false),
                    ReadTimestamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ServerResponse = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailAuditLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FailedADLogonEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkstationName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WorkstationIp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Sid = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
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
