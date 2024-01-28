using BLAZAM.Database.Models.Chat;
using BLAZAM.Database.Models.User;
using BLAZAM.Server.Data;
using BLAZAM.Session.Interfaces;

namespace BLAZAM.Services.Chat
{
    public interface IChatService
    {
        AppEvent<ChatMessage> OnMessagePosted { get; set; }
        AppEvent<AppUser> OnMessageRead { get; set; }
        AppEvent<ChatRoom> OnChatRoomCreated { get; set; }
        ChatRoom? AppChatRoom { get; }
        IQueryable<ChatRoom> ChatRooms { get; }

        void CreateChatRoom(ChatRoom room);
        void DeleteAllChatRooms();
        Task<IQueryable<ChatRoom>> GetChatRoomsAsync();
        /// <summary>
        /// Returns the existing private chat between
        /// these two users. Returns a new chat if no
        /// existing chat exists.
        /// </summary>
        /// <param name="parties">The two parties in the private chat.</param>
        /// <returns></returns>
        ChatRoom GetPrivateTwoWayChat(AppUser currentUser,AppUser otherUser);
        void MessageRead(ChatMessage message, AppUser user);
        void PostMessage(ChatMessage message);
        Task<ChatRoom?> GetChatRoom(ChatRoom? chatRoom);
        List<ChatMessage> GetUnreadMessages(AppUser user);
        List<ChatRoom> GetPrivateChats(AppUser user);
        List<ChatMessage> GetUnreadMessages( AppUser user, ChatRoom room);
    }
}