using BLAZAM.Database.Models.Chat;
using BLAZAM.Database.Models.User;
using BLAZAM.Server.Data;
using BLAZAM.Session.Interfaces;

namespace BLAZAM.Services.Chat
{
    public interface IChatService
    {
        AppEvent<ChatMessage> OnMessagePosted { get; set; }
        AppEvent<IApplicationUserState> OnMessageRead { get; set; }

        void CreateChatRoom(ChatRoom room);
        void DeleteAllChatRooms();
        List<ChatRoom> GetChatRooms();
        Task<List<ChatRoom>> GetChatRoomsAsync();
        /// <summary>
        /// Returns the existing private chat between
        /// these two users. Returns a new chat if no
        /// existing chat exists.
        /// </summary>
        /// <param name="parties">The two parties in the private chat.</param>
        /// <returns></returns>
        ChatRoom GetPrivateTwoWayChat(List<AppUser> parties);
        void MessageRead(ChatMessage message, IApplicationUserState user);
        void PostMessage(ChatMessage message);
        Task<ChatRoom?> GetChatRoom(ChatRoom? chatRoom);
    }
}