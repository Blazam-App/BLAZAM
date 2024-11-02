
namespace BLAZAM.Database.Models.User
{
    public class UserNotification : AppDbSetBase
    {
        public AppUser User { get; set; }
        public NotificationMessage Notification { get; set; }
        public int NotificationId { get; set; }
        public bool IsRead { get; set; }
    }
}