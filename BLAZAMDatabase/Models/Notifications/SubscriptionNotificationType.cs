namespace BLAZAM.Database.Models.Notifications
{
    public class SubscriptionNotificationType : AppDbSetBase
    {
        public NotificationSubscription NotificationSubscription { get; set; }
        public NotificationType NotificationType { get; set; }
    }
}
