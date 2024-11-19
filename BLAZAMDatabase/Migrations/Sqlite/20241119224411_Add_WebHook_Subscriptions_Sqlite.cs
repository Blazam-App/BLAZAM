using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class Add_WebHook_Subscriptions_Sqlite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WebHookSubscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IgnoreSSLVerification = table.Column<bool>(type: "INTEGER", nullable: false),
                    URL = table.Column<string>(type: "TEXT", nullable: false),
                    WebHookMethod = table.Column<int>(type: "INTEGER", nullable: false),
                    WebHookAuthorization = table.Column<int>(type: "INTEGER", nullable: false),
                    AuthorizationToken = table.Column<string>(type: "TEXT", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebHookSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionWebHookType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WebHookSubscriptionId = table.Column<int>(type: "INTEGER", nullable: false),
                    NotificationType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionWebHookType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionWebHookType_WebHookSubscriptions_WebHookSubscriptionId",
                        column: x => x.WebHookSubscriptionId,
                        principalTable: "WebHookSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionWebHookType_WebHookSubscriptionId",
                table: "SubscriptionWebHookType",
                column: "WebHookSubscriptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubscriptionWebHookType");

            migrationBuilder.DropTable(
                name: "WebHookSubscriptions");
        }
    }
}
