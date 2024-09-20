

using BLAZAM.Database.Models.Chat;
using BLAZAM.Database.Models.Notifications;
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
        public List<ReadNewsItem> ReadNewsItems { get; set; } = new();
        public List<UserFavoriteEntry> FavoriteEntries { get; set; } = new();
        //public List<ChatMessage> PostedChatMessages { get; set; } = new();
        //public List<UnreadChatMessage> UnreadChatMessages { get; set; } = new();
        //public List<ChatRoom> ChatRooms{ get; set; } = new();

        public List<UserDashboardWidget> DashboardWidgets { get; set; } = new();
        public List<NotificationSubscription> NotificationSubscriptions { get; set; } = new();
        public byte[]? ProfilePicture { get; set; }
        public string? Email { get; set; }

        public override string? ToString()
        {
            return Username
                ;
        }
    }

}
