using BLAZAM.Database.Models.Notifications;

namespace BLAZAM.Database.Models.User
{
    public class AppUser : AppDbSetBase
    {
        public string UserGUID { get; set; }
        public List<ApiToken>? APITokens { get; set; }
        public string? Theme { get; set; }
        public bool DarkMode { get; set; }
        public bool SearchDisabledUsers { get; set; }
        public bool SearchDisabledComputers { get; set; }
        public string? Username { get; set; }
        public List<UserNotification> Notifications { get; set; } = new();
        public List<ReadNewsItem> ReadNewsItems { get; set; } = new();
        public List<UserFavoriteEntry> FavoriteEntries { get; set; } = new();

        public List<UserDashboardWidget> DashboardWidgets { get; set; } = new();
        public List<NotificationSubscription> NotificationSubscriptions { get; set; } = new();
        public byte[]? ProfilePicture { get; set; }
        public string? Email { get; set; }
        /// <summary>
        /// The authenticator secret for this user in encrypted form
        /// </summary>
        public string? AuthenticatorSecret { get; set; }
        public override string? ToString()
        {
            return Username
                ;
        }
    }

}
