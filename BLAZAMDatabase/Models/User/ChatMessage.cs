using BLAZAM.Database.Models.User;

namespace BLAZAM.Server.Data
{
    public class ChatMessage
    {
        public AppUser User { get; set; }
        public DateTime Timestamp { get; private set; } = DateTime.UtcNow;
        public string Message { get; set; }
        public List<AppUser> SeenBy { get; set; } = new();

    }
}
