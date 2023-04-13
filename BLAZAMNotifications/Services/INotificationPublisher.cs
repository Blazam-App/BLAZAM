using BLAZAM.Database.Models.User;

namespace BLAZAM.Notifications.Services
{
    public interface INotificationPublisher
    {
        AppEvent<List<UserNotification>> OnNotificationPublished { get; set; }

        /// <summary>
        /// Publishes a notification to a single user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="notificationMessage">Can be a new or existing notification</param>
        /// <returns></returns>
        Task PublishNotification(AppUser user, NotificationMessage notificationMessage);

        /// <summary>
        /// Publishes a notification to a list of users
        /// </summary>
        /// <param name="users"></param>
        /// <param name="notificationMessage">Can be a new or existing notification</param>
        /// <returns></returns>
        Task PublishNotification(List<AppUser> users, NotificationMessage notificationMessage);

        /// <summary>
        /// Publishes a notification to all users currently in the users table
        /// </summary>
        /// <param name="notificationMessage">Can be a new or existing notification</param>
        /// <returns></returns>
        Task PublishNotification(NotificationMessage notificationMessage);
    }
}
