using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLAZAM.Database.Migrations.Sql
{
    /// <inheritdoc />
    public partial class Add_ChatSql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Dictionary<AppUser, ChatMessage>ReadByUsersId",
                table: "UserSettings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Dictionary<AppUser, ChatMessage>ReadChatMessagesId",
                table: "UserSettings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ChatRooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MemberCount = table.Column<int>(type: "int", nullable: false),
                    MembersHash = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatRooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppUserChatRoom",
                columns: table => new
                {
                    ChatRoomsId = table.Column<int>(type: "int", nullable: false),
                    MembersId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUserChatRoom", x => new { x.ChatRoomsId, x.MembersId });
                    table.ForeignKey(
                        name: "FK_AppUserChatRoom_ChatRooms_ChatRoomsId",
                        column: x => x.ChatRoomsId,
                        principalTable: "ChatRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppUserChatRoom_UserSettings_MembersId",
                        column: x => x.MembersId,
                        principalTable: "UserSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChatRoomId = table.Column<int>(type: "int", nullable: false),
                    DictionaryAppUserChatMessageReadByUsersId = table.Column<int>(name: "Dictionary<AppUser, ChatMessage>ReadByUsersId", type: "int", nullable: true),
                    DictionaryAppUserChatMessageReadChatMessagesId = table.Column<int>(name: "Dictionary<AppUser, ChatMessage>ReadChatMessagesId", type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessages_ChatRooms_ChatRoomId",
                        column: x => x.ChatRoomId,
                        principalTable: "ChatRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChatMessages_UserSettings_UserId",
                        column: x => x.UserId,
                        principalTable: "UserSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dictionary<AppUser, ChatMessage>",
                columns: table => new
                {
                    ReadByUsersId = table.Column<int>(type: "int", nullable: false),
                    ReadChatMessagesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dictionary<AppUser, ChatMessage>", x => new { x.ReadByUsersId, x.ReadChatMessagesId });
                    table.ForeignKey(
                        name: "FK_Dictionary<AppUser, ChatMessage>_ChatMessages_ReadChatMessagesId",
                        column: x => x.ReadChatMessagesId,
                        principalTable: "ChatMessages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Dictionary<AppUser, ChatMessage>_UserSettings_ReadByUsersId",
                        column: x => x.ReadByUsersId,
                        principalTable: "UserSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSettings_Dictionary<AppUser, ChatMessage>ReadByUsersId_Dictionary<AppUser, ChatMessage>ReadChatMessagesId",
                table: "UserSettings",
                columns: new[] { "Dictionary<AppUser, ChatMessage>ReadByUsersId", "Dictionary<AppUser, ChatMessage>ReadChatMessagesId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppUserChatRoom_MembersId",
                table: "AppUserChatRoom",
                column: "MembersId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ChatRoomId",
                table: "ChatMessages",
                column: "ChatRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_Dictionary<AppUser, ChatMessage>ReadByUsersId_Dictionary<AppUser, ChatMessage>ReadChatMessagesId",
                table: "ChatMessages",
                columns: new[] { "Dictionary<AppUser, ChatMessage>ReadByUsersId", "Dictionary<AppUser, ChatMessage>ReadChatMessagesId" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_UserId",
                table: "ChatMessages",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Dictionary<AppUser, ChatMessage>_ReadChatMessagesId",
                table: "Dictionary<AppUser, ChatMessage>",
                column: "ReadChatMessagesId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSettings_Dictionary<AppUser, ChatMessage>_Dictionary<AppUser, ChatMessage>ReadByUsersId_Dictionary<AppUser, ChatMessage>~",
                table: "UserSettings",
                columns: new[] { "Dictionary<AppUser, ChatMessage>ReadByUsersId", "Dictionary<AppUser, ChatMessage>ReadChatMessagesId" },
                principalTable: "Dictionary<AppUser, ChatMessage>",
                principalColumns: new[] { "ReadByUsersId", "ReadChatMessagesId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_Dictionary<AppUser, ChatMessage>_Dictionary<AppUser, ChatMessage>ReadByUsersId_Dictionary<AppUser, ChatMessage>~",
                table: "ChatMessages",
                columns: new[] { "Dictionary<AppUser, ChatMessage>ReadByUsersId", "Dictionary<AppUser, ChatMessage>ReadChatMessagesId" },
                principalTable: "Dictionary<AppUser, ChatMessage>",
                principalColumns: new[] { "ReadByUsersId", "ReadChatMessagesId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSettings_Dictionary<AppUser, ChatMessage>_Dictionary<AppUser, ChatMessage>ReadByUsersId_Dictionary<AppUser, ChatMessage>~",
                table: "UserSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_ChatRooms_ChatRoomId",
                table: "ChatMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_Dictionary<AppUser, ChatMessage>_Dictionary<AppUser, ChatMessage>ReadByUsersId_Dictionary<AppUser, ChatMessage>~",
                table: "ChatMessages");

            migrationBuilder.DropTable(
                name: "AppUserChatRoom");

            migrationBuilder.DropTable(
                name: "ChatRooms");

            migrationBuilder.DropTable(
                name: "Dictionary<AppUser, ChatMessage>");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropIndex(
                name: "IX_UserSettings_Dictionary<AppUser, ChatMessage>ReadByUsersId_Dictionary<AppUser, ChatMessage>ReadChatMessagesId",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "Dictionary<AppUser, ChatMessage>ReadByUsersId",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "Dictionary<AppUser, ChatMessage>ReadChatMessagesId",
                table: "UserSettings");
        }
    }
}
