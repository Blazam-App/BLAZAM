using BLAZAM.Database.Models.Chat;
using BLAZAM.Server.Data;
using BLAZAM.Session.Interfaces;

namespace BLAZAM.Services.Chat
{
    public interface IChatService
    {
        AppEvent<ChatMessage> OnMessagePosted { get; set; }
        AppEvent<IApplicationUserState> OnMessageRead { get; set; }

        void CreateChatRoom(ChatRoom room);
        List<ChatRoom> GetChatRooms();
        Task<List<ChatRoom>> GetChatRoomsAsync();
        void MessageRead(ChatMessage message, IApplicationUserState user);
        void PostMessage(ChatMessage message);
    }
}