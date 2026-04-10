using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data.Database;
using BLAZAM.Database.Models;
using BLAZAM.Database.Models.Notifications;
using BLAZAM.Database.Models.Permissions;
using BLAZAM.Database.Models.User;
using BLAZAM.EmailMessage.Email.Base;
using BLAZAM.EmailMessage.Email.Messages;
using BLAZAM.Helpers;
using BLAZAM.Localization;
using BLAZAM.Logger;
using BLAZAM.Notifications.Notifications;
using BLAZAM.Notifications.Services;
using BLAZAM.Services.Events;
using BLAZAM.Session.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace BLAZAM.Services.Background
{
    public class NotificationGenerationService
    {
        private readonly IAppDatabaseFactory _databaseFactory;
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
            ApplicationEvents.DirectoryEntryEvent.Delegate += ProcessDirectoryEntryChangedEvent;

        }
        protected virtual void ProcessDirectoryEntryChangedEvent(object? sender, DirectoryEntryChangedArgs args)
        {
            lock (_notificationLock)
            {
                if (!IsSupportedObjectType(args.ObjectType))
                {
                    return;
                }

                if (!IsSupportedEventType(args.EventType))
                {
                    return;
                }

                var isSQLite = _databaseFactory.DatabaseType == DatabaseType.SQLite;
                switch (args.EventType)
                {
                    case ApplicationEventType.Delete:
                        PostNotification(args, NotificationType.Delete, isSQLite);
                        break;
                    case ApplicationEventType.Create:
                        PostNotification(args, NotificationType.Create, isSQLite);
                        break;
                    case ApplicationEventType.PasswordChange:
                        PostNotification(args, NotificationType.PasswordChange, isSQLite);
                        break;
                    case ApplicationEventType.Assign:
                        PostNotification(args, NotificationType.Assign, isSQLite, args.Target);
                        break;
                    case ApplicationEventType.LockedOut:
                        PostNotification(args, NotificationType.LockedOut, isSQLite);
                        break;
                    case ApplicationEventType.Move:
                    case ApplicationEventType.Modify:
                        PostNotification(args, NotificationType.Modify, isSQLite);
                        break;
                    case ApplicationEventType.Unassign:
                        PostNotification(args, NotificationType.Unassign, isSQLite, args.Target);
                        break;
                    case ApplicationEventType.Scheduled:
                        break;
                }
            }
        }

        private bool IsSupportedObjectType(ActiveDirectoryObjectType objectType)
        {
            return objectType == ActiveDirectoryObjectType.Printer ||
                   objectType == ActiveDirectoryObjectType.Computer ||
                   objectType == ActiveDirectoryObjectType.BitLocker ||
                   objectType == ActiveDirectoryObjectType.Group ||
                   objectType == ActiveDirectoryObjectType.OU ||
                   objectType == ActiveDirectoryObjectType.User;
        }

        private bool IsSupportedEventType(ApplicationEventType eventType)
        {
            return eventType == ApplicationEventType.Delete ||
                   eventType == ApplicationEventType.Create ||
                   eventType == ApplicationEventType.PasswordChange ||
                   eventType == ApplicationEventType.Assign ||
                   eventType == ApplicationEventType.LockedOut ||
                   eventType == ApplicationEventType.Move ||
                   eventType == ApplicationEventType.Modify ||
                   eventType == ApplicationEventType.Unassign ||
                   eventType == ApplicationEventType.Scheduled;
        }

        private void PostNotification(DirectoryEntryChangedArgs args, NotificationType notificationType, bool isSQLite, IDirectoryEntryAdapter? target = null)
        {
            if (isSQLite)
            {
                Post(args.Entry, notificationType, args.Actor, target);
            }
            else
            {
                _ = PostAsync(args.Entry, notificationType, args.Actor, target);
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
            await Task.Run(() =>
            {
                Post(source, notificationType, actor, target);
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
            EmailNotificationTemplateComponent? emailMessage;
            PackageNotification(source, notificationType, actor, target, out notification, out notificationTitle, out emailMessage);
            using var context = Context;
            var users = context.UserSettings.Include(us => us.NotificationSubscriptions).ToList();
            if (_databaseFactory.DatabaseType == DatabaseType.SQLite)
            {


                foreach (var user in users)
                {
                    ProcessUserNotification(source, notificationType, actor, user, notification, notificationTitle, emailMessage);
                }
            }
            else
            {
                Parallel.ForEach(users, user =>
                {
                    ProcessUserNotification(source, notificationType, actor, user, notification, notificationTitle, emailMessage);
                });
            }
            PostWebHooks(source, notificationType, actor, target);



        }
        private void ProcessUserNotification(IDirectoryEntryAdapter source,
                                            NotificationType notificationType,
                                            IApplicationUserState? actor,
                                            AppUser user,
                                            NotificationMessage notification,
                                            string notificationTitle,
                                            EmailNotificationTemplateComponent? emailMessage)
        {
            if (user.Id == actor?.Id)
            {
                return;
            }

            var effectiveInAppSubscriptions = CalculateEffectiveInAppSubscriptions(user, source);
            var effectiveEmailSubscriptions = CalculateEffectiveEmailSubscriptions(user, source);

            PublishInAppNotification(user, notificationType, notification, effectiveInAppSubscriptions);
            PublishEmailNotification(user, notificationType, notificationTitle, emailMessage, effectiveEmailSubscriptions);
        }

        private void PublishInAppNotification(
            AppUser user,
            NotificationType notificationType,
            NotificationMessage notification,
            NotificationSubscription? effectiveInAppSubscriptions)
        {
            if (effectiveInAppSubscriptions?.NotificationTypes.Any(x => x.NotificationType == notificationType) == true)
            {
                _ = _notificationPublisher.PublishNotification(user, notification);
            }
        }

        private void PublishEmailNotification(
            AppUser user,
            NotificationType notificationType,
            string notificationTitle,
            EmailNotificationTemplateComponent? emailMessage,
            NotificationSubscription? effectiveEmailSubscriptions)
        {
            if (effectiveEmailSubscriptions?.NotificationTypes.Any(x => x.NotificationType == notificationType) != true)
            {
                return;
            }

            if (emailMessage == null)
            {
                var error = new AppException();
                Loggers.SystemLogger.Error(error, "Email message template was not found!");
                return;
            }

            if (_emailService.IsConfigured && !user.Email.IsNullOrEmpty())
            {
                _ = _emailService.SendMessage(notificationTitle, emailMessage, user.Email);
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
        public void PackageNotification(IDirectoryEntryAdapter source, NotificationType notificationType, IApplicationUserState? actor, IDirectoryEntryAdapter? target, out NotificationMessage notification, out string notificationTitle, out EmailNotificationTemplateComponent? emailMessage)
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
                    createdMessage.EntryLink = source.SearchUri;
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
                    editedMessage.EntryLink = source.SearchUri;

                    emailMessage = editedMessage;
                    break;
                case NotificationType.Unassign:
                    notification.Action = ActiveDirectoryObjectAction.Unassign;

                    notificationTitle += _appLocalization[Lang.Removed_from_Group];
                    notificationBody += _appLocalization["was removed from"] + " <a href=\"" + target.SearchUri + "\" class=\"mud-typography mud-link mud-primary-text mud-link-underline-hover mud-typography-caption\">" + target.CanonicalName + "</a> " + _appLocalization[" at "] + time;

                    var groupMemberRemovedMessage = NotificationType.Unassign.ToNotification<EntryUnassignedEmailMessage>();
                    groupMemberRemovedMessage.EntryName = source.CanonicalName;
                    groupMemberRemovedMessage.EntryLink = source.SearchUri;
                    groupMemberRemovedMessage.GroupName = target?.CanonicalName;
                    emailMessage = groupMemberRemovedMessage;
                    break;
                case NotificationType.Assign:
                    notification.Action = ActiveDirectoryObjectAction.Assign;

                    notificationTitle += _appLocalization[Lang.Added_to_Group];
                    notificationBody += _appLocalization["was assigned to"] + " <a href=\"" + target.SearchUri + "\" class=\"mud-typography mud-link mud-primary-text mud-link-underline-hover mud-typography-caption\">" + target.CanonicalName + "</a> " + _appLocalization[" at "] + time;

                    var groupMemberAssignedMessage = NotificationType.Assign.ToNotification<EntryAssignedEmailMessage>();
                    groupMemberAssignedMessage.EntryName = source.CanonicalName;
                    groupMemberAssignedMessage.EntryLink = source.SearchUri;

                    groupMemberAssignedMessage.GroupName = target?.CanonicalName;

                    emailMessage = groupMemberAssignedMessage;
                    break;
                case NotificationType.PasswordChange:
                    notification.Action = ActiveDirectoryObjectAction.SetPassword;

                    notificationTitle += _appLocalization[Lang.Password_Changed];
                    notificationBody += _appLocalization["had a password reset at "] + time;
                    var passwordChangeMessage = NotificationType.PasswordChange.ToNotification<PasswordChangedEmailMessage>();
                    passwordChangeMessage.EntryName = source.CanonicalName;
                    passwordChangeMessage.EntryLink = source.SearchUri;

                    emailMessage = passwordChangeMessage;
                    break;
                case NotificationType.LockedOut:
                    var sourceUser = source as IADUser;
                    if (sourceUser == null)
                    {
                        return;
                    }

                    notificationTitle += _appLocalization[Lang.Locked_Out];
                    notificationBody += _appLocalization["has been locked out at "] + (sourceUser.LockoutTime!=null?sourceUser.LockoutTime?.ToLocalTime():DateTime.Now);
                    var lockedOutMessage = NotificationType.LockedOut.ToNotification<LockedOutEmailMessage>();
                    lockedOutMessage.EntryName = source.CanonicalName;
                    lockedOutMessage.EntryLink = source.SearchUri;
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
                MessageType = MessageType.EditAccessRequest,
                Title = _appLocalization["Request to"] + " " + _appLocalization[action.ToString()]
            };

            notification.Level = NotificationLevel.Info;
        }

        public void PackageRequest(IDirectoryEntryAdapter target, IActiveDirectoryField field, FieldAccessLevel accessRequested, IApplicationUserState? actor, out NotificationMessage notification)
        {
            if (accessRequested.Id == FieldAccessLevels.Deny.Id)
            {
                throw new ArgumentException("Access requested cannot be Deny", nameof(accessRequested));
            }
            notification = new NotificationMessage()
            {
                //FieldId = (field as ActiveDirectoryField)?.Id,
                //CustomFieldId = (field as CustomActiveDirectoryField)?.Id,
                CreatorId = actor?.Preferences.Id,
                Level = NotificationLevel.Info,
                TargetDN = target.DN,
                MessageType = accessRequested.Id == FieldAccessLevels.Edit.Id ? MessageType.EditAccessRequest : accessRequested.Id == FieldAccessLevels.Read.Id ? MessageType.ReadAccessRequest : MessageType.Notification,
                Title = _appLocalization["Request for"] + " " + _appLocalization[field.DisplayName]
            };

            notification.Level = NotificationLevel.Info;
        }

        private void HandleBlockedEmailSubscriptions(NotificationSubscription effectiveSubscription, NotificationSubscription subscription)
        {
            foreach (var type in subscription.NotificationTypes)
            {
                effectiveSubscription.NotificationTypes.RemoveAll(x => x.NotificationType == type.NotificationType);
            }
        }

        private void HandleAllowedEmailSubscriptions(NotificationSubscription effectiveSubscription, NotificationSubscription subscription)
        {
            var newTypes = subscription.NotificationTypes
                .Where(type => !effectiveSubscription.NotificationTypes.Any(x => x.NotificationType == type.NotificationType))
                .Select(type => new SubscriptionNotificationType { NotificationType = type.NotificationType });

            effectiveSubscription.NotificationTypes.AddRange(newTypes);
        }
        public NotificationSubscription CalculateEffectiveEmailSubscriptions(AppUser user, IDirectoryEntryAdapter ou)
        {
            if (ou is not IADOrganizationalUnit)
            {
                ou = ou.GetParent();
            }

            if (ou is not IADOrganizationalUnit)
            {
                return default;
            }

            using var context = Context;
            var effectiveByEmailSubscription = new NotificationSubscription
            {
                OU = ou.DN,
                User = user,
                ByEmail = true
            };

            var userSubscriptions = context.NotificationSubscriptions
                .Where(x => x.DeletedAt == null && x.UserId == user.Id && ou.DN.Contains(x.OU))
                .OrderBy(x => x.OU)
                .ToList();

            foreach (var sub in userSubscriptions)
            {
                if (!sub.ByEmail)
                {
                    continue;
                }

                if (sub.Block)
                {
                    HandleBlockedEmailSubscriptions(effectiveByEmailSubscription, sub);
                }
                else
                {
                    HandleAllowedEmailSubscriptions(effectiveByEmailSubscription, sub);
                }
            }
            return effectiveByEmailSubscription;
        }

        public NotificationSubscription CalculateEffectiveInAppSubscriptions(AppUser user, IDirectoryEntryAdapter ou)
        {
            if (ou is not IADOrganizationalUnit)
            {
                ou = ou.GetParent();
            }

            if (ou is not IADOrganizationalUnit)
            {
                return default;
            }

            using var context = Context;
            NotificationSubscription effectiveInAppSubscription = new();
            effectiveInAppSubscription = new()
            {
                OU = ou.DN,
                User = user,
                InApp = true
            };

            var userSubscriptions = context.NotificationSubscriptions
                .Where(x => x.DeletedAt == null && x.UserId == user.Id && ou.DN.Contains(x.OU))
                .OrderBy(x => x.OU)
                .ToList();
            foreach (var sub in userSubscriptions)
            {

                if (sub.Block && sub.InApp)
                {
                    foreach (var type in sub.NotificationTypes)
                    {
                        effectiveInAppSubscription.NotificationTypes.RemoveAll(x => x.NotificationType == type.NotificationType);
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
            return effectiveInAppSubscription;
        }
    }
}
