using BLAZAM.Common.Models.Database.User;

namespace BLAZAM.Server.Data
{
    public class ChatMessage
    {
        public AppUser User { get; set; }
        public DateTime Timestamp { get; private set; }
        public string Message { get; set; }

        public ChatMessage()
        {
            Timestamp = DateTime.UtcNow;
        }
    }
}
