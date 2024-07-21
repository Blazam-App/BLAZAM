using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Context;
using BLAZAM.Database.Models.Notifications;
using BLAZAM.Database.Models.User;
using BLAZAM.Localization;
using BLAZAM.Notifications.Services;
using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Services.Background
{
    public class OUNotificationService
    {
        private IAppDatabaseFactory _databaseFactory;
        private readonly INotificationPublisher _notificationPublisher;
        private readonly IStringLocalizer<AppLocalization> _appLocalization;

        public OUNotificationService(IAppDatabaseFactory databaseFactory,INotificationPublisher notificationPublisher, IStringLocalizer<AppLocalization>appLocalization)
        {
            _databaseFactory = databaseFactory;
            _notificationPublisher = notificationPublisher;
            _appLocalization = appLocalization;
        }
        private IDatabaseContext Context => _databaseFactory.CreateDbContext();
        public async Task PostAsync(IDirectoryEntryAdapter source, NotificationType notificationType)
        {
            await Task.Delay(150);
            var context = Context;
            string notificationTitle;
            notificationTitle = _appLocalization[source.ObjectType.ToString()] + " ";

            string notificationBody;
            notificationBody = "<a href=\"" + source.SearchUri + "\">" + source.CanonicalName + "</a> ";

            switch (notificationType)
            {
                case NotificationType.Create:
                    notificationTitle += _appLocalization["Created"];
                    notificationBody += _appLocalization["was created at"] + source.Created.Value.ToLocalTime();

                    break;
                    case NotificationType.Delete:
                    notificationTitle += _appLocalization["Deleted"];
                    notificationBody += _appLocalization["was deleted at"] + source.LastChanged.Value.ToLocalTime();

                    break;
                case NotificationType.Modify:
                    notificationTitle += _appLocalization["Modified"];
                    notificationBody += _appLocalization["was modified at"] + source.LastChanged.Value.ToLocalTime();

                    break;
                case NotificationType.GroupAssignment:
                    notificationTitle += _appLocalization["Group Membership Changed"];
                    notificationBody += _appLocalization["was modified at"] + source.LastChanged.Value.ToLocalTime();

                    break;
                case NotificationType.PasswordChange:
                    notificationTitle += _appLocalization["Password Reset"];
                    notificationBody += _appLocalization["had a password reset at"] + source.LastChanged.Value.ToLocalTime();

                    break;
                    
            }
            var notification = new NotificationMessage();
            notification.Title = notificationTitle;
            notification.Message = notificationBody;
            notification.Dismissable = true;
            notification.Created = DateTime.Now;
            notification.Level = NotificationLevel.Info;

            foreach (var user in Context.UserSettings.ToList())
            {
                var effectiveInAppSubscriptions = CalculateEffectiveInAppSubscriptions(user, source);
                var effectiveEmailSubscriptions = CalculateEffectiveEmailSubscriptions(user, source);
                if (effectiveInAppSubscriptions.NotificationTypes.Any(x => x.NotificationType == notificationType))
                {
                    _notificationPublisher.PublishNotification(user, notification);
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
