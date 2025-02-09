using BLAZAM.Server.Data;

namespace BLAZAM.Database.Models.Chat
{
    public class UnreadChatMessage : AppDbSetBase
    {
        public ChatMessage ChatMessage { get; set; }
        //This  causes a cyclic FK loop
        //public ChatRoom ChatRoom { get; set; }
        //This  causes a cyclic FK loop
        //public AppUser User { get; set; }
        public int ChatRoomId { get; set; }
        public int ChatMessageId { get; set; }
        public int UserId { get; set; }
    }
}
