using BLAZAM.Database.Context;
using BLAZAM.Database.Models.User;
using BLAZAM.Logger;
using Microsoft.EntityFrameworkCore;

namespace BLAZAM.Notifications.Services
{
    public class NotificationPublisher : INotificationPublisher
    {
        private readonly IAppDatabaseFactory _databaseFactory;
        public AppDelegate<List<UserNotification>> OnNotificationPublished { get; set; }
        public AppDelegate OnNotificationDeleted { get; set; }
        public NotificationPublisher(IAppDatabaseFactory databaseFactory)
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
                try
                {

                    context.NotificationMessages.Add(notificationMessage);
                    context.SaveChanges();
                    var id = notificationMessage.Id;
                    notificationMessage = context.NotificationMessages.First(nm=>nm.Id.Equals(notificationMessage.Id));
                }
                catch (Exception ex)
                {
                    Loggers.SystemLogger.Error("Error saving new notification {Error}", ex);

                }
            }
            else
            {
                notificationMessage = context.NotificationMessages.FirstOrDefault(m => m.Id.Equals(notificationMessage.Id));
            }
            List<UserNotification> sentNotifications = new();
            foreach (var user in users)
            {
                var userNotification = new UserNotification()
                {
                    User = context.UserSettings.Where(u => u.Equals(user)).FirstOrDefault(),
                    NotificationId = notificationMessage.Id
                };
                context.UserNotifications.Add(userNotification);
                sentNotifications.Add(userNotification);
            }
            try
            {
                context.SaveChanges();

                OnNotificationPublished?.Invoke(sentNotifications);

            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error("Error saving new usernotification {Error}", ex);
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

        public async Task<bool> DeleteNotification(NotificationMessage notificationMessage, AppUser user)
        {
            try
            {
                using var context = await _databaseFactory.CreateDbContextAsync();
                var dbNotification = await context.NotificationMessages.FirstOrDefaultAsync(x => x.Id == notificationMessage.Id);

                context.NotificationMessages.Remove(dbNotification);
                await context.SaveChangesAsync();
                this.OnNotificationDeleted.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error("Error deleting notification {Error}", ex);
                return false;
            }
        }
    }
}
