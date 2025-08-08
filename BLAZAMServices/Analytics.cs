using BLAZAM.Database.Context;
using BLAZAM.Logger;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace BLAZAM.Services
{
    /// <summary>
    /// Service for posting application analytics events, typically to Google Analytics via JS interop.
    /// </summary>
    public class Analytics
    {
        private readonly IAppDatabaseFactory _dbFactory;
        private readonly IJSRuntime _jsRuntime;

        protected string? ProductionGA4Property
        {
            get
            {
                using var context = _dbFactory.CreateDbContext(); // Create and dispose context
                try
                {
                    return context.AppSettings.FirstOrDefault()?.AnalyticsId;
                }
                catch (System.Exception ex)
                {
                    Loggers.DatabaseLogger.Error(ex, "Analytics: Error accessing AppSettings for ProductionGA4Property.");
                    return null;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Analytics"/> class.
        /// </summary>
        /// <param name="dbFactory">The database context factory.</param>
        /// <param name="jSRuntime">The JavaScript runtime for JS interop.</param>
        /// <exception cref="ArgumentNullException">If dbFactory or jSRuntime is null.</exception>
        public Analytics(IAppDatabaseFactory dbFactory, IJSRuntime jSRuntime)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory), "dbFactory cannot be null in Analytics constructor.");
            _jsRuntime = jSRuntime ?? throw new ArgumentNullException(nameof(jSRuntime), "jSRuntime cannot be null in Analytics constructor.");
        }

        /// <summary>
        /// Posts an analytics event for an object being moved.
        /// </summary>
        /// <param name="objectType">The type of the Active Directory object that was moved.</param>
        public async Task ObjectMoved(ActiveDirectoryObjectType objectType)
        {
            await PostCustomEvent("object_moved", objectType.ToString());
        }

        /// <summary>
        /// Posts an analytics event for an object being created.
        /// </summary>
        /// <param name="objectType">The type of the Active Directory object that was created.</param>
        public async Task ObjectCreated(ActiveDirectoryObjectType objectType)
        {
            await PostCustomEvent("object_created", objectType.ToString());
        }

        /// <summary>
        /// Posts an analytics event for an object being deleted.
        /// </summary>
        /// <param name="objectType">The type of the Active Directory object that was deleted.</param>
        public async Task ObjectDeleted(ActiveDirectoryObjectType objectType)
        {
            await PostCustomEvent("object_deleted", objectType.ToString());
        }

        /// <summary>
        /// Posts an analytics event for an object being renamed.
        /// </summary>
        /// <param name="objectType">The type of the Active Directory object that was renamed.</param>
        public async Task ObjectRenamed(ActiveDirectoryObjectType objectType)
        {
            await PostCustomEvent("object_renamed", objectType.ToString());
        }

        /// <summary>
        /// Posts an analytics event for an object being enabled.
        /// </summary>
        /// <param name="objectType">The type of the Active Directory object that was enabled.</param>
        public async Task ObjectEnabled(ActiveDirectoryObjectType objectType)
        {
            await PostCustomEvent("object_enabled", objectType.ToString());
        }

        /// <summary>
        /// Posts an analytics event for an object being disabled.
        /// </summary>
        /// <param name="objectType">The type of the Active Directory object that was disabled.</param>
        public async Task ObjectDisabled(ActiveDirectoryObjectType objectType)
        {
            await PostCustomEvent("object_disabled", objectType.ToString());
        }

        /// <summary>
        /// Posts an analytics event for an object being unlocked.
        /// </summary>
        /// <param name="objectType">The type of the Active Directory object that was unlocked.</param>
        public async Task ObjectUnlocked(ActiveDirectoryObjectType objectType)
        {
            await PostCustomEvent("object_unlocked", objectType.ToString());
        }

        /// <summary>
        /// Posts an analytics event for an object being assigned.
        /// </summary>
        /// <param name="objectType">The type of the Active Directory object that was assigned.</param>
        public async Task ObjectAssigned(ActiveDirectoryObjectType objectType)
        {
            await PostCustomEvent("object_assigned", objectType.ToString());
        }

        /// <summary>
        /// Posts an analytics event for an object being unassigned.
        /// </summary>
        /// <param name="objectType">The type of the Active Directory object that was unassigned.</param>
        public async Task ObjectUnassigned(ActiveDirectoryObjectType objectType)
        {
            await PostCustomEvent("object_unassigned", objectType.ToString());
        }

        /// <summary>
        /// Posts an analytics event for an object being restored.
        /// </summary>
        /// <param name="objectType">The type of the Active Directory object that was restored.</param>
        public async Task ObjectRestored(ActiveDirectoryObjectType objectType)
        {
            await PostCustomEvent("object_restored", objectType.ToString());
        }

        /// <summary>
        /// Posts an analytics event for an object's history being viewed.
        /// </summary>
        /// <param name="objectType">The type of the Active Directory object whose history was viewed.</param>
        public async Task ObjectHistoryViewed(ActiveDirectoryObjectType objectType)
        {
            await PostCustomEvent("object_history_viewed", objectType.ToString());
        }

        /// <summary>
        /// Posts an analytics event for an object's password being reset.
        /// </summary>
        /// <param name="objectType">The type of the Active Directory object whose password was reset.</param>
        public async Task ObjectPasswordReset(ActiveDirectoryObjectType objectType)
        {
            await PostCustomEvent("object_password_reset", objectType.ToString());
        }

        /// <summary>
        /// Posts an analytics event for the finalization of the application installation.
        /// </summary>
        public async Task InstallationFinalized()
        {
            await PostCustomEvent("installation_finalized");
        }

        /// <summary>
        /// Posts an analytics event for a new rule being created.
        /// </summary>
        /// <param name="ruleName">The name of the rule that was created.</param>
        public async Task RuleCreated(string ruleName)
        {
            await PostCustomEvent("rule_created", ruleName);
        }

        /// <summary>
        /// Posts an analytics event for a rule being executed.
        /// </summary>
        /// <param name="ruleName">The name of the rule that was executed.</param>
        /// <param name="success">A boolean indicating whether the rule execution was successful.</param>
        public async Task RuleExecuted(string ruleName, bool success)
        {
            await PostCustomEvent("rule_executed", new { ruleName, success });
        }

        /// <summary>
        /// Posts an analytics event for a new webhook being created.
        /// </summary>
        /// <param name="webhookName">The name of the webhook that was created.</param>
        public async Task WebhookCreated(string webhookName)
        {
            await PostCustomEvent("webhook_created", webhookName);
        }

        /// <summary>
        /// Posts an analytics event for a webhook being executed.
        /// </summary>
        /// <param name="webhookName">The name of the webhook that was executed.</param>
        /// <param name="success">A boolean indicating whether the webhook execution was successful.</param>
        public async Task WebhookExecuted(string webhookName, bool success)
        {
            await PostCustomEvent("webhook_executed", new { webhookName, success });
        }

        /// <summary>
        /// Posts an analytics event for a new access request being created.
        /// </summary>
        /// <param name="requestDetails">Details about the access request.</param>
        public async Task AccessRequestCreated(string requestDetails)
        {
            await PostCustomEvent("access_request_created", requestDetails);
        }

        /// <summary>
        /// Posts an analytics event for an access request being approved.
        /// </summary>
        /// <param name="requestDetails">Details about the approved access request.</param>
        public async Task AccessRequestApproved(string requestDetails)
        {
            await PostCustomEvent("access_request_approved", requestDetails);
        }

        /// <summary>
        /// Posts an analytics event for an access request being denied.
        /// </summary>
        /// <param name="requestDetails">Details about the denied access request.</param>
        public async Task AccessRequestDenied(string requestDetails)
        {
            await PostCustomEvent("access_request_denied", requestDetails);
        }

        /// <summary>
        /// Posts an analytics event for a notification being dismissed.
        /// </summary>
        /// <param name="notificationType">The type of the notification that was dismissed.</param>
        public async Task NotificationDismissed(string notificationType)
        {
            await PostCustomEvent("notification_dismissed", notificationType);
        }

        /// <summary>
        /// Posts a custom analytics event to the underlying JavaScript interop.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="data">Optional data payload for the event (will be JSON serialized).</param>
        public async Task PostCustomEvent(string eventName, object? data = null)
        {
            
            Loggers.SystemLogger.Debug("Analytics: Posting custom event '{EventName}' with data: {Data}", eventName, data == null ? "null" : JsonConvert.SerializeObject(data));
            try
            {
                if (data == null)
                {
                    await _jsRuntime.InvokeVoidAsync("customAnalyticsEvent", new object[] { eventName });
                }
                else
                {
                    await _jsRuntime.InvokeVoidAsync("customAnalyticsEvent", new object[] { eventName, JsonConvert.SerializeObject(new object[] { data }) });

                }
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Warning("Error attepmting to send Google Analytics event {EventName}{Error}", eventName, ex);
            }
        }
    }
}
