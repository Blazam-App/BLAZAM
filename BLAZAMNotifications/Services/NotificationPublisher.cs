using BLAZAM.Database.Context;
using BLAZAM.Database.Models.User;
using BLAZAM.Logger;
using Microsoft.EntityFrameworkCore;

namespace BLAZAM.Notifications.Services
{
    /// <summary>
    /// Service responsible for publishing notifications to users by creating entries in the database and invoking events for real-time updates.
    /// </summary>
    public class NotificationPublisher : INotificationPublisher
    {
        private readonly IAppDatabaseFactory _databaseFactory;

        /// <summary>
        /// Delegate for handling notification published events.
        /// </summary>
        public AppDelegate<List<UserNotification>> OnNotificationPublished { get; set; }

        /// <summary>
        /// Delegate for handling notification deleted events.
        /// </summary>
        public AppDelegate OnNotificationDeleted { get; set; }

        /// <summary>Initializes a new instance of the <see cref="NotificationPublisher"/> class.</summary> 
        /// <param name="databaseFactory">Factory for creating database context instances.</param> 
        /// <exception cref="ArgumentNullException">Thrown if databaseFactory is null.</exception>
        public NotificationPublisher(IAppDatabaseFactory databaseFactory)
        {
            ArgumentNullException.ThrowIfNull(databaseFactory);

            _databaseFactory = databaseFactory;
        }

        /// <summary>Publishes a notification to a single user.</summary> 
        /// <param name="user">The user to receive the notification.</param> 
        /// <param name="notificationMessage">The notification message to publish. If its Id is 0, it will be added to the database.</param> 
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task PublishNotification(AppUser user, NotificationMessage notificationMessage)
            => PublishNotification(new List<AppUser> { user }, notificationMessage);

        /// <summary>Publishes a notification to a list of users.</summary> 
        /// <param name="users">The list of users to receive the notification. Null users or users not found in DB will be skipped.</param> 
        /// <param name="notificationMessage">The notification message to publish. If its Id is 0, it will be added to the database.</param> 
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task PublishNotification(List<AppUser> users, NotificationMessage notificationMessage)
        {
            if (users == null)
            {
                Loggers.SystemLogger.Warning("NotificationPublisher.PublishNotification: 'users' list is null. Cannot publish notification.");
                return Task.CompletedTask;
            }
            if (notificationMessage == null)
            {
                Loggers.SystemLogger.Warning("NotificationPublisher.PublishNotification: 'notificationMessage' is null. Cannot publish notification.");
                return Task.CompletedTask;
            }

            using var context = _databaseFactory.CreateDbContext();

            notificationMessage = EnsureNotificationMessage(context, notificationMessage);
            if (notificationMessage == null)
            {
                // Error already logged in EnsureNotificationMessage
                return Task.CompletedTask;
            }

            var sentNotifications = CreateUserNotifications(context, users, notificationMessage);

            if (sentNotifications.Any())
            {
                try
                {
                    context.SaveChanges();
                    OnNotificationPublished?.Invoke(sentNotifications);
                }
                catch (Exception ex)
                {
                    Loggers.SystemLogger.Error(ex, "Error saving new user notifications {Error}", ex.Message);
                }
            }
            return Task.CompletedTask;
        }

        private NotificationMessage? EnsureNotificationMessage(IDatabaseContext context, NotificationMessage notificationMessage)
        {
            if (notificationMessage.Id == 0)
            {
                try
                {
                    context.NotificationMessages.Add(notificationMessage);
                    context.SaveChanges();
                    return context.NotificationMessages.First(nm => nm.Id == notificationMessage.Id);
                }
                catch (Exception ex)
                {
                    Loggers.SystemLogger.Error(ex, "Error saving new notification {Error}", ex.Message);
                    return null;
                }
            }
            else
            {
                var existingMessage = context.NotificationMessages.FirstOrDefault(m => m.Id == notificationMessage.Id);
                if (existingMessage == null)
                {
                    Loggers.SystemLogger.Warning("NotificationPublisher.PublishNotification: NotificationMessage with ID {NotificationId} not found in database. Cannot publish.", notificationMessage.Id);
                    return null;
                }
                return existingMessage;
            }
        }

        private List<UserNotification> CreateUserNotifications(IDatabaseContext context, List<AppUser> users, NotificationMessage notificationMessage)
        {
            List<UserNotification> sentNotifications = new();
            foreach (var user in users)
            {
                if (user == null)
                {
                    Loggers.SystemLogger.Warning("NotificationPublisher.PublishNotification: Encountered a null AppUser in the 'users' list. Skipping.");
                    continue;
                }
                var dbUser = context.UserSettings.FirstOrDefault(u => u.Id == user.Id);
                if (dbUser == null)
                {
                    Loggers.SystemLogger.Warning("NotificationPublisher.PublishNotification: User with ID {UserId} (Username: {Username}) provided in the list was not found in the database. Skipping notification for this user.", user.Id, user.Username);
                    continue;
                }
                var userNotification = new UserNotification()
                {
                    User = dbUser,
                    NotificationId = notificationMessage.Id
                };
                context.UserNotifications.Add(userNotification);
                sentNotifications.Add(userNotification);
            }
            return sentNotifications;
        }

        /// <summary>Publishes a notification to all users currently in the UserSettings table.</summary> 
        /// <param name="notificationMessage">The notification message to publish. If its Id is 0, it will be added to the database.</param> 
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task PublishNotification(NotificationMessage notificationMessage)
        {
            using var context = _databaseFactory.CreateDbContext();
            var allUsers = context.UserSettings.ToList(); // Materialize the list
            return PublishNotification(allUsers, notificationMessage); // Return the task
        }

        /// <summary>Deletes a specific notification message. In the current implementation, this removes the root message, affecting all users who received it.</summary> 
        /// <param name="notificationMessage">The notification message to delete. Must not be null.</param> 
        /// <param name="user">The user initiating the delete (currently unused in logic but good for context or future permissions). Must not be null.</param> 
        /// <returns>True if deletion was successful, false otherwise.</returns>
        public async Task<bool> DeleteNotification(NotificationMessage notificationMessage, AppUser user)
        {
            if (notificationMessage == null)
            {
                Loggers.SystemLogger.Warning("NotificationPublisher.DeleteNotification: 'notificationMessage' is null. Cannot delete notification.");
                return await Task.FromResult(false); // Corrected to await Task.FromResult
            }
            if (user == null)
            {
                Loggers.SystemLogger.Warning("NotificationPublisher.DeleteNotification: 'user' is null. Cannot delete notification for unspecified user.");
                return await Task.FromResult(false); // Corrected to await Task.FromResult
            }
            try
            {
                using var context = await _databaseFactory.CreateDbContextAsync();
                var dbNotification = await context.NotificationMessages.FirstOrDefaultAsync(x => x.Id == notificationMessage.Id);
                if (dbNotification == null)
                {
                    Loggers.SystemLogger.Warning("NotificationPublisher.DeleteNotification: NotificationMessage with ID {NotificationId} not found in database. Cannot delete.", notificationMessage.Id);
                    return false;
                }
                context.NotificationMessages.Remove(dbNotification);
                await context.SaveChangesAsync();
                this.OnNotificationDeleted?.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex, "Error deleting notification with ID {NotificationId}. Error: {ErrorMessage}", notificationMessage.Id, ex.Message); // Use ex.Message and include ID
                return false;
            }
        }
    }
}
