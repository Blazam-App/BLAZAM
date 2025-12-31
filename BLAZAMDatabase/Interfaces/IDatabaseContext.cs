using BLAZAM.Common.Data.Database;
using BLAZAM.Database.Models;
using BLAZAM.Database.Models.Audit;
using BLAZAM.Database.Models.Chat;
using BLAZAM.Database.Models.Notifications;
using BLAZAM.Database.Models.Permissions;
using BLAZAM.Database.Models.Rules;
using BLAZAM.Database.Models.Templates;
using BLAZAM.Database.Models.User;
using BLAZAM.Global.Enums;
using Microsoft.EntityFrameworkCore;

namespace BLAZAM.Database.Interfaces
{
    public interface IDatabaseContext : IEFCoreDbContext
    {
        /// <summary>
        /// All the applied field access mappings for Deny, Read, Write.
        /// </summary>
        DbSet<FieldAccessMapping> AccessLevelFieldMapping { get; set; }

        /// <summary>
        /// All the applied object access mappings for Deny, Read.
        /// </summary>
        DbSet<ObjectAccessMapping> AccessLevelObjectMapping { get; set; }

        /// <summary>
        /// The access levels available in the application.
        /// </summary>
        DbSet<AccessLevel> AccessLevels { get; set; }

        /// <summary>
        /// The built-in Active Directory fields available for mapping and permissions.
        /// </summary>
        DbSet<ActiveDirectoryField> ActiveDirectoryFields { get; set; }

        /// <summary>
        /// The settings for connecting to Active Directory.
        /// </summary>
        DbSet<ADSettings> ActiveDirectorySettings { get; set; }

        /// <summary>
        /// The base application settings, including installation and configuration.
        /// </summary>
        DbSet<AppSettings> AppSettings { get; set; }

        /// <summary>
        /// The authentication settings for the application.
        /// </summary>
        DbSet<AuthenticationSettings> AuthenticationSettings { get; set; }

        /// <summary>
        /// The field values for directory templates.
        /// </summary>
        DbSet<DirectoryTemplateFieldValue> DirectoryTemplateFieldValues { get; set; }

        /// <summary>
        /// The groups of directory templates for organizing templates.
        /// </summary>
        DbSet<DirectoryTemplateGroup> DirectoryTemplateGroups { get; set; }

        /// <summary>
        /// The directory templates used for creating directory entries.
        /// </summary>
        DbSet<DirectoryTemplate> DirectoryTemplates { get; set; }

        /// <summary>
        /// The email settings for outgoing mail configuration.
        /// </summary>
        DbSet<EmailSettings> EmailSettings { get; set; }

        /// <summary>
        /// The templates for email messages sent by the application.
        /// </summary>
        DbSet<EmailTemplate> EmailTemplates { get; set; }

        /// <summary>
        /// The field access levels for permissions on individual fields.
        /// </summary>
        DbSet<FieldAccessLevel> FieldAccessLevel { get; set; }

        /// <summary>
        /// The audit log for user logon events.
        /// </summary>
        DbSet<LogonAuditLog> LogonAuditLog { get; set; }

        /// <summary>
        /// The object access levels for permissions on objects.
        /// </summary>
        DbSet<ObjectAccessLevel> ObjectAccessLevel { get; set; }

        /// <summary>
        /// The available object actions for permission mapping.
        /// </summary>
        DbSet<ObjectAction> ObjectActionFlag { get; set; }

        /// <summary>
        /// The permission delegates for assigning permissions to users or groups.
        /// </summary>
        DbSet<PermissionDelegate> PermissionDelegate { get; set; }

        /// <summary>
        /// The permission mappings for users, groups, and roles.
        /// </summary>
        DbSet<PermissionMapping> PermissionMap { get; set; }

        /// <summary>
        /// The audit log for permission changes.
        /// </summary>
        DbSet<PermissionsAuditLog> PermissionsAuditLog { get; set; }

        /// <summary>
        /// The audit log for directory entry requests.
        /// </summary>
        DbSet<RequestAuditLog> RequestAuditLog { get; set; }

        /// <summary>
        /// The audit log for application settings changes.
        /// </summary>
        DbSet<SettingsAuditLog> SettingsAuditLog { get; set; }

        /// <summary>
        /// The audit log for system-level events.
        /// </summary>
        DbSet<SystemAuditLog> SystemAuditLog { get; set; }

        /// <summary>
        /// The application user settings and profiles.
        /// </summary>
        DbSet<AppUser> UserSettings { get; set; }

        /// <summary>
        /// The last error encountered during a save operation.
        /// </summary>
        Exception? LastSaveError { get; set; }

        /// <summary>
        /// The connection string as set in the ASP Net Core appsettings.json.
        /// <para>This should be set before any attempts to connect.</para>
        /// <para>Usually in the Program.Main method before injecting the service.</para>
        /// </summary>
        DatabaseConnectionString? ConnectionString { get; }

        /// <summary>
        /// Checks the real-time pingability and connectivity to the database right now.
        /// </summary>
        ServiceConnectionState Status { get; }

        /// <summary>
        /// The notification messages sent to users.
        /// </summary>
        DbSet<NotificationMessage> NotificationMessages { get; set; }

        /// <summary>
        /// The notifications assigned to individual users.
        /// </summary>
        DbSet<UserNotification> UserNotifications { get; set; }

        /// <summary>
        /// Indicates if there is a mismatch between applied and pending seed migrations.
        /// </summary>
        bool SeedMismatch { get; }

        /// <summary>
        /// The custom Active Directory fields defined by administrators.
        /// </summary>
        DbSet<CustomActiveDirectoryField> CustomActiveDirectoryFields { get; set; }

        /// <summary>
        /// The dashboard widgets assigned to users.
        /// </summary>
        DbSet<UserDashboardWidget> UserDashboardWidgets { get; set; }

        /// <summary>
        /// The audit log for changes to directory entries.
        /// </summary>
        DbSet<DirectoryEntryAuditLog> DirectoryEntryAuditLogs { get; set; }

        /// <summary>
        /// The chat rooms available in the application.
        /// </summary>
        DbSet<ChatRoom> ChatRooms { get; set; }

        /// <summary>
        /// The chat messages sent within chat rooms.
        /// </summary>
        DbSet<ChatMessage> ChatMessages { get; set; }

        /// <summary>
        /// The unread chat messages for users.
        /// </summary>
        DbSet<UnreadChatMessage> UnreadChatMessages { get; set; }

        /// <summary>
        /// The mappings between Active Directory fields and object types.
        /// </summary>
        DbSet<ActiveDirectoryFieldObjectType> ActiveDirectoryFieldObjectMappings { get; set; }

        /// <summary>
        /// The notification subscriptions for users and groups.
        /// </summary>
        DbSet<NotificationSubscription> NotificationSubscriptions { get; set; }

        /// <summary>
        /// The global permission settings for the application.
        /// </summary>
        DbSet<GlobalPermissionSettings> GlobalPermissionSettings { get; set; }

        /// <summary>
        /// The global permission request actions available for approval workflows.
        /// </summary>
        DbSet<GlobalPermissionRequestAction> GlobalPermissionRequestActions { get; set; }

        /// <summary>
        /// The API tokens issued for programmatic access.
        /// </summary>
        DbSet<ApiToken> ApiTokens { get; set; }

        /// <summary>
        /// The webhook subscriptions for external integrations.
        /// </summary>
        DbSet<WebHookSubscription> WebHookSubscriptions { get; set; }

        /// <summary>
        /// The attempts made to deliver webhooks.
        /// </summary>
        DbSet<WebHookAttempt> WebHookAttempts { get; set; }

        /// <summary>
        /// The list of users currently locked out of the system.
        /// </summary>
        DbSet<LockedOutUser> LockedOutUsers { get; set; }

        /// <summary>
        /// The audit log for email events and deliveries.
        /// </summary>
        DbSet<EmailAuditLog> EmailAuditLog { get; set; }

        /// <summary>
        /// The events for failed Active Directory logons.
        /// </summary>
        DbSet<FailedADLogonEvent> FailedADLogonEvents { get; set; }

        /// <summary>
        /// The automation rules for workflow and process automation.
        /// </summary>
        DbSet<AutomationRule> AutomationRules { get; set; }

        /// <summary>
        /// The field values used in automation rule actions.
        /// </summary>
        DbSet<AutomationRuleActionFieldValue> AutomationRuleFieldValues { get; set; }

        /// <summary>
        /// The OR filters used in automation rules.
        /// </summary>
        DbSet<AutomationRuleOrFilter> AutomationRuleOrFilter { get; set; }

        /// <summary>
        /// The AND filters used in automation rules.
        /// </summary>
        DbSet<AutomationRuleAndFilter> AutomationRuleAndFilters { get; set; }

        /// <summary>
        /// The group SIDs used in automation rules for targeting groups.
        /// </summary>
        DbSet<AutomationRuleGroupGuid> AutomationRuleGroupGuids { get; set; }

        /// <summary>
        /// The actions performed by automation rules.
        /// </summary>
        DbSet<AutomationRuleAction> AutomationRuleActions { get; set; }
        DbSet<GlobalAutomationRuleSettings> GlobalAutomationRuleSettings { get; set; }
        DbSet<GlobalPermissionRequestField> GlobalPermissionRequestFields { get; set; }
        DbSet<AutomationRuleAuditLog> AutomationRuleAuditLog { get; set; }

        /// <summary>
        /// Exports all database tables to CSV files in the specified directory.
        /// </summary>
        /// <param name="directory">The directory to export CSV files to.</param>
        void Export(string directory);
    }
}