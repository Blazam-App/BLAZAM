using AngleSharp.Dom;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using BLAZAM.Common.Data.Database;
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
using BLAZAM.Server.Data.Services;
using BLAZAM.Session.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;

namespace BLAZAM.Services.Background
{
    public class NotificationGenerationService
    {
        private IAppDatabaseFactory _databaseFactory;
        private readonly INotificationPublisher _notificationPublisher;
        private readonly IStringLocalizer<AppLocalization> _appLocalization;
        private readonly EmailService _emailService;
        private readonly WebHookPublisher _webHookPublisher;

        public NotificationGenerationService(IAppDatabaseFactory databaseFactory, INotificationPublisher notificationPublisher, IStringLocalizer<AppLocalization> appLocalization, EmailService emailService, WebHookPublisher webHookPublisher)
        {
            _databaseFactory = databaseFactory;
            _notificationPublisher = notificationPublisher;
            _appLocalization = appLocalization;
            _emailService = emailService;
            _webHookPublisher = webHookPublisher;
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
                        await ProcessUserNotification(source, notificationType, actor, user, notification, notificationTitle, emailMessage, _emailConfigured);
                    }
                }
                else
                {
                    Parallel.ForEach(users, async user =>
                    {
                        await ProcessUserNotification(source, notificationType, actor, user, notification, notificationTitle, emailMessage, _emailConfigured);
                    });
                }
                PostWebHooks(source, notificationType, actor, target);

            });

        }

        private async Task ProcessUserNotification(IDirectoryEntryAdapter source, NotificationType notificationType, IApplicationUserState? actor, AppUser user, NotificationMessage notification, string notificationTitle, NotificationTemplateComponent? emailMessage, bool _emailConfigured)
        {
            //Avoid sending to triggering user if actor is set
            if (user.Id != actor?.Id)
            {
                var effectiveInAppSubscriptions = CalculateEffectiveInAppSubscriptions(user, source);
                var effectiveEmailSubscriptions = CalculateEffectiveEmailSubscriptions(user, source);

                if (effectiveInAppSubscriptions!=null && effectiveInAppSubscriptions.NotificationTypes.Any(x => x.NotificationType == notificationType))
                {
                    await _notificationPublisher.PublishNotification(user, notification);
                }

                if (effectiveEmailSubscriptions!=null && effectiveEmailSubscriptions.NotificationTypes.Any(x => x.NotificationType == notificationType))
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

        private async Task PostWebHooks(IDirectoryEntryAdapter source, NotificationType notificationType, IApplicationUserState? actor = null, IDirectoryEntryAdapter? target = null)
        {
            using var context = Context;
            var webhooks = await context.WebHookSubscriptions.Where(w => w.DeletedAt == null)
                .Include(w => w.NotificationTypes)
                .Where(x => x.DeletedAt == null)
                .ToListAsync();
            if (webhooks.Any(w => w.NotificationTypes.Any(nt => nt.NotificationType == notificationType)))
            {
                var subscribedWebhooks = webhooks.Where(w => w.NotificationTypes.Any(nt => nt.NotificationType == notificationType));
                if (_databaseFactory.DatabaseType == DatabaseType.SQLite)
                {
                    foreach(var webhook in subscribedWebhooks)
                    {
                        _webHookPublisher.PublishWebhook(webhook, source, notificationType, actor, target);
                    }
                }
                else
                {
                    Parallel.ForEach(subscribedWebhooks, async webhook =>
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

                    notificationTitle += _appLocalization["Created"];
                    notificationBody += _appLocalization["was created at "] + time;
                    var createdMessage = NotificationType.Create.ToNotification<EntryCreatedEmailMessage>();
                    createdMessage.EntryName = source.CanonicalName;
                    emailMessage = createdMessage;
                    break;
                case NotificationType.Delete:
                    notification.Action = ActiveDirectoryObjectAction.Delete;

                    notificationTitle += _appLocalization["Deleted"];
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

                    notificationTitle += _appLocalization["Removed from Group"];
                    notificationBody += _appLocalization["was removed from"] + " <a href=\"" + target.SearchUri + "\" class=\"mud-typography mud-link mud-primary-text mud-link-underline-hover mud-typography-caption\">" + target.CanonicalName + "</a> " + _appLocalization[" at "] + time;

                    var groupMemberRemovedMessage = NotificationType.Unassign.ToNotification<EntryUnassignedEmailMessage>();
                    groupMemberRemovedMessage.EntryName = source?.CanonicalName;
                    groupMemberRemovedMessage.GroupName = target?.CanonicalName;
                    emailMessage = groupMemberRemovedMessage;
                    break;
                case NotificationType.Assign:
                    notification.Action = ActiveDirectoryObjectAction.Assign;

                    notificationTitle += _appLocalization["Added to Group"];
                    notificationBody += _appLocalization["was assigned to"] + " <a href=\"" + target.SearchUri + "\" class=\"mud-typography mud-link mud-primary-text mud-link-underline-hover mud-typography-caption\">" + target.CanonicalName + "</a> " + _appLocalization[" at "] + time;

                    var groupMemberAssignedMessage = NotificationType.Assign.ToNotification<EntryAssignedEmailMessage>();
                    groupMemberAssignedMessage.EntryName = source?.CanonicalName;
                    groupMemberAssignedMessage.GroupName = target?.CanonicalName;

                    emailMessage = groupMemberAssignedMessage;
                    break;
                case NotificationType.PasswordChange:
                    notification.Action = ActiveDirectoryObjectAction.SetPassword;

                    notificationTitle += _appLocalization["Password Reset"];
                    notificationBody += _appLocalization["had a password reset at "] + time;
                    var passwordChangeMessage = NotificationType.PasswordChange.ToNotification<PasswordChangedEmailMessage>();
                    passwordChangeMessage.EntryName = source.CanonicalName;
                    emailMessage = passwordChangeMessage;
                    break;
                case NotificationType.LockedOut:
                    var sourceUser = source as IADUser;
                    if (sourceUser == null) return;
                    notificationTitle += _appLocalization["User locked out"];
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
            notification.CreatorId = actor?.Preferences.Id;
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
