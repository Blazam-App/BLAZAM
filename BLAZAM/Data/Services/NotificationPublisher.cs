
using BLAZAM.Database.Context;
using BLAZAM.Database.Models.User;
using BLAZAM.Notifications.Services;

namespace BLAZAM.Server.Data.Services
{
    public class NotificationPublisher : INotificationPublisher
    {
        private readonly AppDatabaseFactory _databaseFactory;
        public AppEvent<List<UserNotification>> OnNotificationPublished { get; set; }
        public NotificationPublisher(AppDatabaseFactory databaseFactory)
        {
            _databaseFactory = databaseFactory;
        }

        public Task PublishNotification(AppUser user, NotificationMessage notificationMessage)
            => PublishNotification(new List<AppUser> { user }, notificationMessage);
        

        public Task PublishNotification(List<AppUser> users, NotificationMessage notificationMessage)
        {
            using var context = _databaseFactory.CreateDbContext();

            if (notificationMessage.Id == 0)
            {
                notificationMessage = context.NotificationMessages.Add(notificationMessage).Entity;
                context.SaveChanges();
            }
            List<UserNotification> sentNotificaitons = new();
            foreach(var user in users)
            {
                var userNotification = new UserNotification()
                {
                    User = context.UserSettings.Where(u=>u.Equals(user)).FirstOrDefault(),
                    Notification = notificationMessage
                };
                context.UserNotifications.Add(userNotification);
                sentNotificaitons.Add(userNotification);
            }
            try
            {
                context.SaveChanges();

                OnNotificationPublished?.Invoke(sentNotificaitons);

            }
            catch (Exception ex)
            {

            }
            return Task.CompletedTask;

        }

        public Task PublishNotification(NotificationMessage notificationMessage)
        {
            using var context = _databaseFactory.CreateDbContext();
            var allUsers = context.UserSettings.ToList();
            PublishNotification(allUsers, notificationMessage);
            return Task.CompletedTask;
        }
    }
}
