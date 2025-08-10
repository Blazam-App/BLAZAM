using BLAZAM.Database.Models.User;

namespace BLAZAM.Database.Models.Chat
{
    public class ChatMessage : AppDbSetBase
    {
        public AppUser User { get; set; }
        public DateTime Timestamp { get; private set; } = DateTime.UtcNow;
        public string Message { get; set; }
        public ChatRoom ChatRoom { get; set; }
        public int ChatRoomId { get; set; }
        public List<UnreadChatMessage> NotReadBy { get; set; }

    }
}
