

using BLAZAM.Database.Models.Chat;
using BLAZAM.Server.Data;

namespace BLAZAM.Database.Models.User
{
    public class AppUser : AppDbSetBase
    {
        public string UserGUID { get; set; }
        public string? APIToken { get; set; }
        public string? Theme { get; set; }
        public bool DarkMode { get; set; }
        public bool SearchDisabledUsers { get; set; }
        public bool SearchDisabledComputers { get; set; }
        public string? Username { get; set; }
        public List<UserNotification> Messages { get; set; } = new();
        //public List<ChatMessage> PostedChatMessages { get; set; } = new();
        //public List<ReadChatMessage> ReadChatMessages { get; set; } = new();
        //public List<ChatRoom> ChatRooms{ get; set; } = new();
        
        public List<UserDashboardWidget> DashboardWidgets { get; set; }= new();
        public byte[]? ProfilePicture { get; set; }
        public string? Email { get; set; }
    }

}
