using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Context;
using BLAZAM.Database.Models.Notifications;
using BLAZAM.Database.Models.User;
using BLAZAM.EmailMessage.Email.Base;
using BLAZAM.EmailMessage.Email.Notifications;
using BLAZAM.Helpers;
using BLAZAM.Localization;
using BLAZAM.Logger;
using BLAZAM.Notifications.Notifications;
using BLAZAM.Notifications.Services;
using Microsoft.Extensions.Localization;

namespace BLAZAM.Services.Background
{
    public class OUNotificationService
    {
        private IAppDatabaseFactory _databaseFactory;
        private readonly INotificationPublisher _notificationPublisher;
        private readonly IStringLocalizer<AppLocalization> _appLocalization;
        private readonly EmailService _emailService;

        public OUNotificationService(IAppDatabaseFactory databaseFactory, INotificationPublisher notificationPublisher, IStringLocalizer<AppLocalization> appLocalization, EmailService emailService)
        {
            _databaseFactory = databaseFactory;
            _notificationPublisher = notificationPublisher;
            _appLocalization = appLocalization;
            _emailService = emailService;
        }
        private IDatabaseContext Context => _databaseFactory.CreateDbContext();
        public async Task PostAsync(IDirectoryEntryAdapter source, NotificationType notificationType)
        {
            await Task.Delay(150);
            var context = Context;
            string notificationTitle;
            notificationTitle = _appLocalization[source.ObjectType.ToString()] + " ";

            string notificationBody;
            NotificationTemplateComponent? emailMessage = null;
            notificationBody = "<a href=\"" + source.SearchUri + "\">" + source.CanonicalName + "</a> ";

            switch (notificationType)
            {
                case NotificationType.Create:
                    notificationTitle += _appLocalization["Created"];
                    notificationBody += _appLocalization["was created at"] + source.Created?.ToLocalTime();
                    var createdMessage = NotificationType.Create.ToNotification<EntryCreatedEmailMessage>();
                    createdMessage.EntryName = source.CanonicalName;
                    emailMessage = createdMessage;
                    break;
                case NotificationType.Delete:
                    notificationTitle += _appLocalization["Deleted"];
                    notificationBody += _appLocalization["was deleted at"] + source.LastChanged?.ToLocalTime();
                    var deletedMessage = NotificationType.Delete.ToNotification<EntryDeletedEmailMessage>();
                    deletedMessage.EntryName = source.CanonicalName;
                    emailMessage = deletedMessage;
                    break;
                case NotificationType.Modify:
                    notificationTitle += _appLocalization["Modified"];
                    notificationBody += _appLocalization["was modified at"] + source.LastChanged?.ToLocalTime();
                    var editedMessage = NotificationType.Modify.ToNotification<EntryEditedEmailMessage>();
                    editedMessage.EntryName = source.CanonicalName;
                    emailMessage = editedMessage;
                    break;
                case NotificationType.GroupAssignment:
                    notificationTitle += _appLocalization["Group Membership Changed"];
                    notificationBody += _appLocalization["was modified at"] + source.LastChanged?.ToLocalTime();
                  
                    var groupMembershipMessage = NotificationType.GroupAssignment.ToNotification<EntryGroupAssignmentEmailMessage>();
                    groupMembershipMessage.EntryName = source.CanonicalName;
                    emailMessage = groupMembershipMessage;
                    break;
                case NotificationType.PasswordChange:
                    notificationTitle += _appLocalization["Password Reset"];
                    notificationBody += _appLocalization["had a password reset at"] + source.LastChanged?.ToLocalTime();
                    var passwordChangeMessage = NotificationType.PasswordChange.ToNotification<PasswordChangedEmailMessage>();
                    passwordChangeMessage.EntryName = source.CanonicalName;
                    emailMessage = passwordChangeMessage;
                    break;

            }
            var notification = new NotificationMessage();
            notification.Title = notificationTitle;
            notification.Message = notificationBody;
            notification.Dismissable = true;
            notification.Created = DateTime.Now;
            notification.Level = NotificationLevel.Info;
            var _emailConfigured = _emailService.IsConfigured;

            foreach (var user in Context.UserSettings.ToList())
            {
                var effectiveInAppSubscriptions = CalculateEffectiveInAppSubscriptions(user, source);
                var effectiveEmailSubscriptions = CalculateEffectiveEmailSubscriptions(user, source);
                if (effectiveInAppSubscriptions.NotificationTypes.Any(x => x.NotificationType == notificationType))
                {
                    await _notificationPublisher.PublishNotification(user, notification);
                }
                if (effectiveEmailSubscriptions.NotificationTypes.Any(x => x.NotificationType == notificationType))
                {
                    if (emailMessage != null)
                    {
                        if (_emailConfigured && !user.Email.IsNullOrEmpty())
                        {
                            await _emailService.SendMessage(notificationTitle, emailMessage, user.Email);

                        }
                    }
                    else
                    {
                        var error = new ApplicationException();
                        Loggers.SystemLogger.Error("Email message template was not found! {@Error}", error);
                    }
                }
            }
        }
        public NotificationSubscription CalculateEffectiveEmailSubscriptions(AppUser user, IDirectoryEntryAdapter ou)
        {
            if (ou is not IADOrganizationalUnit)
                ou = ou.GetParent();
            if (ou is not IADOrganizationalUnit)
                return default;
            var context = Context;
            NotificationSubscription effectiveByEmailSubscription = new();

            effectiveByEmailSubscription = new();
            effectiveByEmailSubscription.OU = ou.DN;
            effectiveByEmailSubscription.User = user;
            effectiveByEmailSubscription.ByEmail = true;
            var userSubscriptions = context.NotificationSubscriptions.Where(x => x.DeletedAt == null && x.UserId == user.Id).ToList();
            userSubscriptions = userSubscriptions.OrderBy(x => x.OU).ToList();
            foreach (var sub in userSubscriptions)
            {
                try
                {
                    if (ou.DN.Contains(sub.OU))
                    {
                        if (sub.Block)
                        {

                            if (sub.ByEmail)
                            {
                                foreach (var type in sub.NotificationTypes)
                                {
                                    effectiveByEmailSubscription.NotificationTypes.RemoveAll(x => x.NotificationType == type.NotificationType);
                                }
                            }
                        }
                        else
                        {

                            if (sub.ByEmail)
                            {

                                foreach (var type in sub.NotificationTypes)
                                {
                                    if (!effectiveByEmailSubscription.NotificationTypes.Any(x => x.NotificationType == type.NotificationType))
                                    {
                                        effectiveByEmailSubscription.NotificationTypes.Add(new() { NotificationType = type.NotificationType });
                                    }
                                }
                            }


                        }
                    }
                }
                catch (Exception ex)
                {
                    Loggers.SystemLogger.Error("Error while parsing users for notification broadcast {@Error}", ex);
                }
            }
            return effectiveByEmailSubscription;
        }

        public NotificationSubscription CalculateEffectiveInAppSubscriptions(AppUser user, IDirectoryEntryAdapter ou)
        {
            if (ou is not IADOrganizationalUnit)
                ou = ou.GetParent();
            if (ou is not IADOrganizationalUnit)
                return default;
            var context = Context;
            NotificationSubscription effectiveInAppSubscription = new();
            effectiveInAppSubscription = new();
            effectiveInAppSubscription.OU = ou.DN;
            effectiveInAppSubscription.User = user;
            effectiveInAppSubscription.InApp = true;

            var userSubscriptions = context.NotificationSubscriptions.Where(x => x.DeletedAt == null && x.UserId == user.Id).ToList();
            userSubscriptions = userSubscriptions.OrderBy(x => x.OU).ToList();
            foreach (var sub in userSubscriptions)
            {
                if (ou.DN.Contains(sub.OU))
                {
                    if (sub.Block)
                    {
                        if (sub.InApp)
                        {
                            foreach (var type in sub.NotificationTypes)
                            {
                                effectiveInAppSubscription.NotificationTypes.RemoveAll(x => x.NotificationType == type.NotificationType);
                            }
                        }

                    }
                    else
                    {
                        if (sub.InApp)
                        {
                            foreach (var type in sub.NotificationTypes)
                            {
                                if (!effectiveInAppSubscription.NotificationTypes.Any(x => x.NotificationType == type.NotificationType))
                                {
                                    effectiveInAppSubscription.NotificationTypes.Add(new() { NotificationType = type.NotificationType });
                                }
                            }
                        }



                    }
                }
            }
            return effectiveInAppSubscription;
        }
    }
}
