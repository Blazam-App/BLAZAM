using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data.Database;
using BLAZAM.Common.Exceptions;
using BLAZAM.Database.Context;
using BLAZAM.Database.Models.Notifications;
using BLAZAM.Database.Models.Permissions;
using BLAZAM.Database.Models.User;
using BLAZAM.EmailMessage.Email.Base;
using BLAZAM.EmailMessage.Email.Notifications;
using BLAZAM.Helpers;
using BLAZAM.Localization;
using BLAZAM.Logger;
using BLAZAM.Notifications.Notifications;
using BLAZAM.Notifications.Services;
using BLAZAM.Services.Events;
using BLAZAM.Session.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Linq;
using Octokit;
using Serilog.Parsing;
using System.Text.RegularExpressions;

namespace BLAZAM.Services.Background
{
    public class NotificationGenerationService
    {
        private IAppDatabaseFactory _databaseFactory;
        private readonly INotificationPublisher _notificationPublisher;
        private readonly IStringLocalizer<AppLocalization> _appLocalization;
        private readonly EmailService _emailService;
        private readonly WebHookPublisher _webHookPublisher;
        private readonly object _notificationLock = new();
        public NotificationGenerationService(IAppDatabaseFactory databaseFactory, INotificationPublisher notificationPublisher, IStringLocalizer<AppLocalization> appLocalization, EmailService emailService, WebHookPublisher webHookPublisher)
        {
            _databaseFactory = databaseFactory;
            _notificationPublisher = notificationPublisher;
            _appLocalization = appLocalization;
            _emailService = emailService;
            _webHookPublisher = webHookPublisher;
            ApplicationEvents.DirectoryEntryChanged.Delegate += ProcessDirectoryEntryChangedEvent;

        }
        protected virtual void ProcessDirectoryEntryChangedEvent(object? sender, DirectoryEntryChangedArgs args)
        {
            lock (_notificationLock)
            {
                switch (args.ObjectType)
                {
                    case ActiveDirectoryObjectType.Printer:
                    case ActiveDirectoryObjectType.Computer:
                    case ActiveDirectoryObjectType.BitLocker:
                    case ActiveDirectoryObjectType.Group:
                    case ActiveDirectoryObjectType.OU:
                    case ActiveDirectoryObjectType.User:
                        switch (args.EventType)
                        {
                            case ApplicationEventType.Delete:
                                if (_databaseFactory.DatabaseType == DatabaseType.SQLite)
                                {
                                    Post(args.Entry, NotificationType.Delete, args.Actor);

                                }
                                else
                                {
                                    _ = PostAsync(args.Entry, NotificationType.Delete, args.Actor);

                                }
                                break;
                            case ApplicationEventType.Create:
                                if (_databaseFactory.DatabaseType == DatabaseType.SQLite)
                                {
                                    Post(args.Entry, NotificationType.Create, args.Actor);

                                }
                                else
                                {
                                    _ = PostAsync(args.Entry, NotificationType.Create, args.Actor);

                                }
                                break;
                            case ApplicationEventType.PasswordChange:
                                if (_databaseFactory.DatabaseType == DatabaseType.SQLite)
                                {
                                    Post(args.Entry, NotificationType.Create, args.Actor);

                                }
                                else
                                {
                                    _=PostAsync(args.Entry, NotificationType.Create, args.Actor);

                                }
                                break;
                            case ApplicationEventType.Assign:
                                if (_databaseFactory.DatabaseType == DatabaseType.SQLite)
                                {
                                    Post(args.Entry, NotificationType.Assign, args.Actor, args.Target);

                                }
                                else
                                {
                                    _ = PostAsync(args.Entry, NotificationType.Assign, args.Actor, args.Target);

                                }
                                break;
                            case ApplicationEventType.LockedOut:

                                if (_databaseFactory.DatabaseType == DatabaseType.SQLite)
                                {
                                    Post(args.Entry, NotificationType.LockedOut);

                                }
                                else
                                {
                                    _ = PostAsync(args.Entry, NotificationType.LockedOut);

                                }
                                break;
                            case ApplicationEventType.Move:
                            case ApplicationEventType.Modify:
                                if (_databaseFactory.DatabaseType == DatabaseType.SQLite)
                                {
                                    Post(args.Entry, NotificationType.Modify,args.Actor);

                                }
                                else
                                {
                                    _ = PostAsync(args.Entry, NotificationType.Modify, args.Actor);

                                }
                                break;
                            case ApplicationEventType.Unassign:
                                if (_databaseFactory.DatabaseType == DatabaseType.SQLite)
                                {
                                    Post(args.Entry, NotificationType.Unassign,args.Actor,args.Target);

                                }
                                else
                                {
                                    _ = PostAsync(args.Entry, NotificationType.Unassign, args.Actor, args.Target);

                                }
                                break;
                            case ApplicationEventType.Scheduled:
                                break;
                        }
                        break;
                }
            }
        }
        private IDatabaseContext Context => _databaseFactory.CreateDbContext();

        /// <summary>
        /// Post a notification to OU subscribers
        /// </summary>
        /// <param name="source"></param>
        /// <param name="notificationType"></param>
        /// <param name="actor"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public async Task PostAsync(IDirectoryEntryAdapter source, NotificationType notificationType, IApplicationUserState? actor = null, IDirectoryEntryAdapter? target = null)
        {
            await Task.Run(async () =>
            {
                PostAsync(source, notificationType, actor, target);
            });

        }
        /// <summary>
        /// Post a notification to OU subscribers
        /// </summary>
        /// <param name="source"></param>
        /// <param name="notificationType"></param>
        /// <param name="actor"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public void Post(IDirectoryEntryAdapter source, NotificationType notificationType, IApplicationUserState? actor = null, IDirectoryEntryAdapter? target = null)
        {
           
                NotificationMessage notification;
                string notificationTitle;
                NotificationTemplateComponent? emailMessage;
                PackageNotification(source, notificationType, actor, target, out notification, out notificationTitle, out emailMessage);
                var _emailConfigured = _emailService.IsConfigured;
                using var context = Context;
                var users = context.UserSettings.Include(us => us.NotificationSubscriptions).ToList();
                if (_databaseFactory.DatabaseType == DatabaseType.SQLite)
                {


                    foreach (var user in users)
                    {
                        ProcessUserNotification(source, notificationType, actor, user, notification, notificationTitle, emailMessage, _emailConfigured);
                    }
                }
                else
                {
                    Parallel.ForEach(users, async user =>
                    {
                        ProcessUserNotification(source, notificationType, actor, user, notification, notificationTitle, emailMessage, _emailConfigured);
                    });
                }
                PostWebHooks(source, notificationType, actor, target);

           

        }
        private async Task ProcessUserNotification(IDirectoryEntryAdapter source, NotificationType notificationType, IApplicationUserState? actor, AppUser user, NotificationMessage notification, string notificationTitle, NotificationTemplateComponent? emailMessage, bool _emailConfigured)
        {
            //Avoid sending to triggering user if actor is set
            if (user.Id != actor?.Id)
            {
                //Calculate recipient subscriptions
                var effectiveInAppSubscriptions = CalculateEffectiveInAppSubscriptions(user, source);
                var effectiveEmailSubscriptions = CalculateEffectiveEmailSubscriptions(user, source);

                //Publish in app notifications to subscribing subscriptions
                if (effectiveInAppSubscriptions != null && effectiveInAppSubscriptions.NotificationTypes.Any(x => x.NotificationType == notificationType))
                {
                    _ =  _notificationPublisher.PublishNotification(user, notification);
                }

                //Publish email notification to subscribing subscriptions
                if (effectiveEmailSubscriptions != null && effectiveEmailSubscriptions.NotificationTypes.Any(x => x.NotificationType == notificationType))
                {
                    if (emailMessage != null)
                    {
                        if (_emailConfigured && !user.Email.IsNullOrEmpty())
                        {
                            _= _emailService.SendMessage(notificationTitle, emailMessage, user.Email);
                        }
                    }
                    else
                    {
                        var error = new AppException();
                        Loggers.SystemLogger.Error("Email message template was not found! {@Error}", error);
                    }
                }
            }
        }

        private async Task PostWebHooks(IDirectoryEntryAdapter source, NotificationType notificationType, IApplicationUserState? actor = null, IDirectoryEntryAdapter? target = null)
        {
            using var context = Context;
            var webhooks = await context.WebHookSubscriptions.Where(w => w.DeletedAt == null)
                .Include(w => w.NotificationTypes)
                .ToListAsync();
            if (webhooks.Any(w => w.NotificationTypes.Any(nt => nt.NotificationType == notificationType)))
            {
                var subscribedWebhooks = webhooks.Where(w => w.NotificationTypes.Any(nt => nt.NotificationType == notificationType));
                if (_databaseFactory.DatabaseType == DatabaseType.SQLite)
                {
                    foreach (var webhook in subscribedWebhooks)
                    {
                        _webHookPublisher.PublishWebhook(webhook, source, notificationType, actor, target);
                    }
                }
                else
                {
                    Parallel.ForEach(subscribedWebhooks, webhook =>
                    {
                        _webHookPublisher.PublishWebhook(webhook, source, notificationType, actor, target);
                    });
                }


            }
        }
        /// <summary>
        /// Package a notification from event parameters
        /// </summary>
        /// <param name="source"></param>
        /// <param name="notificationType"></param>
        /// <param name="actor"></param>
        /// <param name="target"></param>
        /// <param name="notification"></param>
        /// <param name="notificationTitle"></param>
        /// <param name="emailMessage"></param>
        public void PackageNotification(IDirectoryEntryAdapter source, NotificationType notificationType, IApplicationUserState? actor, IDirectoryEntryAdapter? target, out NotificationMessage notification, out string notificationTitle, out NotificationTemplateComponent? emailMessage)
        {
            notification = new NotificationMessage();
            notificationTitle = _appLocalization[source.ObjectType.ToString()] + " ";

            string notificationBody;
            emailMessage = null;
            notificationBody = "<a href=\"" + source.SearchUri + "\" class=\"mud-typography mud-link mud-primary-text mud-link-underline-hover mud-typography-caption\">" + source.CanonicalName + "</a> ";
            var time = DateTime.Now.ToString();
            switch (notificationType)
            {
                case NotificationType.Create:
                    notification.Action = ActiveDirectoryObjectAction.Create;

                    notificationTitle += _appLocalization[Lang.Created];
                    notificationBody += _appLocalization["was created at "] + time;
                    var createdMessage = NotificationType.Create.ToNotification<EntryCreatedEmailMessage>();
                    createdMessage.EntryName = source.CanonicalName;
                    emailMessage = createdMessage;
                    break;
                case NotificationType.Delete:
                    notification.Action = ActiveDirectoryObjectAction.Delete;

                    notificationTitle += _appLocalization[Lang.Deleted];
                    notificationBody += _appLocalization["was deleted at "] + time;
                    var deletedMessage = NotificationType.Delete.ToNotification<EntryDeletedEmailMessage>();
                    deletedMessage.EntryName = source.CanonicalName;
                    emailMessage = deletedMessage;
                    break;
                case NotificationType.Modify:
                    notificationTitle += _appLocalization["Modified"];
                    notificationBody += _appLocalization["was modified at "] + time;

                    var editedMessage = NotificationType.Modify.ToNotification<EntryEditedEmailMessage>();
                    editedMessage.EntryName = source.CanonicalName;
                    emailMessage = editedMessage;
                    break;
                case NotificationType.Unassign:
                    notification.Action = ActiveDirectoryObjectAction.Unassign;

                    notificationTitle += _appLocalization[Lang.Removed_from_Group];
                    notificationBody += _appLocalization["was removed from"] + " <a href=\"" + target.SearchUri + "\" class=\"mud-typography mud-link mud-primary-text mud-link-underline-hover mud-typography-caption\">" + target.CanonicalName + "</a> " + _appLocalization[" at "] + time;

                    var groupMemberRemovedMessage = NotificationType.Unassign.ToNotification<EntryUnassignedEmailMessage>();
                    groupMemberRemovedMessage.EntryName = source?.CanonicalName;
                    groupMemberRemovedMessage.GroupName = target?.CanonicalName;
                    emailMessage = groupMemberRemovedMessage;
                    break;
                case NotificationType.Assign:
                    notification.Action = ActiveDirectoryObjectAction.Assign;

                    notificationTitle += _appLocalization[Lang.Added_to_Group];
                    notificationBody += _appLocalization["was assigned to"] + " <a href=\"" + target.SearchUri + "\" class=\"mud-typography mud-link mud-primary-text mud-link-underline-hover mud-typography-caption\">" + target.CanonicalName + "</a> " + _appLocalization[" at "] + time;

                    var groupMemberAssignedMessage = NotificationType.Assign.ToNotification<EntryAssignedEmailMessage>();
                    groupMemberAssignedMessage.EntryName = source?.CanonicalName;
                    groupMemberAssignedMessage.GroupName = target?.CanonicalName;

                    emailMessage = groupMemberAssignedMessage;
                    break;
                case NotificationType.PasswordChange:
                    notification.Action = ActiveDirectoryObjectAction.SetPassword;

                    notificationTitle += _appLocalization[Lang.Password_Changed];
                    notificationBody += _appLocalization["had a password reset at "] + time;
                    var passwordChangeMessage = NotificationType.PasswordChange.ToNotification<PasswordChangedEmailMessage>();
                    passwordChangeMessage.EntryName = source.CanonicalName;
                    emailMessage = passwordChangeMessage;
                    break;
                case NotificationType.LockedOut:
                    var sourceUser = source as IADUser;
                    if (sourceUser == null) return;
                    notificationTitle += _appLocalization[Lang.Locked_Out];
                    notificationBody += _appLocalization["has been locked out at "] + sourceUser.LockoutTime?.ToLocalTime();
                    var lockedOutMessage = NotificationType.LockedOut.ToNotification<LockedOutEmailMessage>();
                    lockedOutMessage.EntryName = source.CanonicalName;
                    emailMessage = lockedOutMessage;
                    break;

            }
            if (actor != null)
            {
                notificationBody += " " + _appLocalization["by"] + " " + actor.AuditUsername;
            }
            notification.Title = notificationTitle;
            notification.Message = notificationBody;
            notification.Dismissable = true;
            notification.CreatorId = actor?.Preferences?.Id;
            notification.Level = NotificationLevel.Info;
        }

        public void PackageRequest(IDirectoryEntryAdapter target, ActiveDirectoryObjectAction action, IApplicationUserState? actor, out NotificationMessage notification)
        {
            notification = new NotificationMessage()
            {
                Action = action,
                CreatorId = actor?.Preferences.Id,
                Level = NotificationLevel.Info,
                TargetDN = target.DN,
                MessageType = MessageType.AccessRequest,
                Title = _appLocalization["Request to"] + " " + _appLocalization[action.ToString()]
            };

            notification.Level = NotificationLevel.Info;
        }

        public NotificationSubscription CalculateEffectiveEmailSubscriptions(AppUser user, IDirectoryEntryAdapter ou)
        {
            if (ou is not IADOrganizationalUnit)
                ou = ou.GetParent();
            if (ou is not IADOrganizationalUnit)
                return default;
            using var context = Context;
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
                                effectiveByEmailSubscription.NotificationTypes.AddRange(from type in sub.NotificationTypes
                                                                                        where !effectiveByEmailSubscription.NotificationTypes.Any(x => x.NotificationType == type.NotificationType)
                                                                                        select new SubscriptionNotificationType() { NotificationType = type.NotificationType });
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
            using var context = Context;
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
