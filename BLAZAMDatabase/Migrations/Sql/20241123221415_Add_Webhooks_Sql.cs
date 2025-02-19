using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Database.Migrations.Sql
{
    /// <inheritdoc />
    public partial class Add_Webhooks_Sql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WebHookSubscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OU = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IgnoreSSLVerification = table.Column<bool>(type: "bit", nullable: false),
                    URL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WebHookMethod = table.Column<int>(type: "int", nullable: false),
                    WebHookSignature = table.Column<int>(type: "int", nullable: false),
                    WebHookAuthorization = table.Column<int>(type: "int", nullable: false),
                    AuthorizationToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HmacKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrivateKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PublicKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebHookSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionWebHookType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WebHookSubscriptionId = table.Column<int>(type: "int", nullable: false),
                    NotificationType = table.Column<int>(type: "int", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "WebHookAttempts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastAttemptTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WebHookSubscriptionId = table.Column<int>(type: "int", nullable: false),
                    Uri = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Delivered = table.Column<bool>(type: "bit", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Signature = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    ResponseCode = table.Column<int>(type: "int", nullable: false),
                    ResponseHeaders = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseMessage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebHookAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebHookAttempts_WebHookSubscriptions_WebHookSubscriptionId",
                        column: x => x.WebHookSubscriptionId,
                        principalTable: "WebHookSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionWebHookType_WebHookSubscriptionId",
                table: "SubscriptionWebHookType",
                column: "WebHookSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_WebHookAttempts_WebHookSubscriptionId",
                table: "WebHookAttempts",
                column: "WebHookSubscriptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubscriptionWebHookType");

            migrationBuilder.DropTable(
                name: "WebHookAttempts");

            migrationBuilder.DropTable(
                name: "WebHookSubscriptions");
        }
    }
}
