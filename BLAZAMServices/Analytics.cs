using AngleSharp.Dom;
using BLAZAM.Database.Context;
using BLAZAM.Logger;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System.DirectoryServices;
using System.Net.Http;
using System.Text;

namespace BLAZAM.Services
{
    /// <summary>
    /// Service for posting application analytics events, typically to Google Analytics via JS interop.
    /// Added optional server-side GA4 Measurement Protocol support.
    /// </summary>
    public class Analytics
    {
        private readonly IAppDatabaseFactory _dbFactory;
        private readonly IJSRuntime _jsRuntime;
        private readonly HttpClient? _httpClient;

        /// <summary>
        /// Gets the Google Analytics 4 property ID for the production environment.
        /// </summary>
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
        /// HttpClient is optional — if provided, server-side posting (Measurement Protocol) is available.
        /// </summary>
        /// <param name="dbFactory">The database context factory.</param>
        /// <param name="jSRuntime">The JavaScript runtime for JS interop.</param>
        /// <param name="httpClient">Optional HttpClient for server-side analytics (GA4 Measurement Protocol).</param>
        /// <exception cref="ArgumentNullException">If dbFactory or jSRuntime is null.</exception>
        public Analytics(IAppDatabaseFactory dbFactory, IJSRuntime jSRuntime, HttpClient? httpClient = null)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory), "dbFactory cannot be null in Analytics constructor.");
            _jsRuntime = jSRuntime ?? throw new ArgumentNullException(nameof(jSRuntime), "jSRuntime cannot be null in Analytics constructor.");
            _httpClient = httpClient;
        }

        /// <summary>
        /// Posts an analytics event for an object being moved.
        /// </summary>
        /// <param name="objectType">The type of the Active Directory object that was moved.</param>
        public async Task ObjectMoved(ActiveDirectoryObjectType objectType)
        {
            await PostClientSideCustomEvent("object_moved", objectType.ToString());
        }

        /// <summary>
        /// Posts an analytics event for an object being created.
        /// </summary>
        /// <param name="objectType">The type of the Active Directory object that was created.</param>
        public async Task ObjectCreated(ActiveDirectoryObjectType objectType)
        {
            await PostClientSideCustomEvent("object_created", objectType.ToString());
        }

        /// <summary>
        /// Sends a client-side custom event indicating that an Active Directory object has been modified.
        /// </summary>
        /// <remarks>This method triggers an asynchronous operation to post the event. Ensure that the
        /// caller handles the task appropriately.</remarks>
        /// <param name="objectType">The type of the Active Directory object that was modified. This value is converted to a string and included
        /// in the event data.</param>
        /// <returns></returns>
        public async Task ObjectModified(ActiveDirectoryObjectType objectType)
        {
            await PostClientSideCustomEvent("object_modified", objectType.ToString());
        }

        /// <summary>
        /// Posts an analytics event for an object being deleted.
        /// </summary>
        /// <param name="objectType">The type of the Active Directory object that was deleted.</param>
        public async Task ObjectDeleted(ActiveDirectoryObjectType objectType)
        {
            await PostClientSideCustomEvent("object_deleted", objectType.ToString());
        }

        /// <summary>
        /// Posts an analytics event for an object being renamed.
        /// </summary>
        /// <param name="objectType">The type of the Active Directory object that was renamed.</param>
        public async Task ObjectRenamed(ActiveDirectoryObjectType objectType)
        {
            await PostClientSideCustomEvent("object_renamed", objectType.ToString());
        }

        /// <summary>
        /// Posts an analytics event for an object being enabled.
        /// </summary>
        /// <param name="objectType">The type of the Active Directory object that was enabled.</param>
        public async Task ObjectEnabled(ActiveDirectoryObjectType objectType)
        {
            await PostClientSideCustomEvent("object_enabled", objectType.ToString());
        }

        /// <summary>
        /// Posts an analytics event for an object being disabled.
        /// </summary>
        /// <param name="objectType">The type of the Active Directory object that was disabled.</param>
        public async Task ObjectDisabled(ActiveDirectoryObjectType objectType)
        {
            await PostClientSideCustomEvent("object_disabled", objectType.ToString());
        }

        /// <summary>
        /// Posts an analytics event for an object being unlocked.
        /// </summary>
        /// <param name="objectType">The type of the Active Directory object that was unlocked.</param>
        public async Task ObjectUnlocked(ActiveDirectoryObjectType objectType)
        {
            await PostClientSideCustomEvent("object_unlocked", objectType.ToString());
        }

        /// <summary>
        /// Posts an analytics event for an object being assigned.
        /// </summary>
        /// <param name="objectType">The type of the Active Directory object that was assigned.</param>
        public async Task ObjectAssigned(ActiveDirectoryObjectType objectType)
        {
            await PostClientSideCustomEvent("object_assigned", objectType.ToString());
        }

        /// <summary>
        /// Posts an analytics event for an object being unassigned.
        /// </summary>
        /// <param name="objectType">The type of the Active Directory object that was unassigned.</param>
        public async Task ObjectUnassigned(ActiveDirectoryObjectType objectType)
        {
            await PostClientSideCustomEvent("object_unassigned", objectType.ToString());
        }

        /// <summary>
        /// Posts an analytics event for an object being restored.
        /// </summary>
        /// <param name="objectType">The type of the Active Directory object that was restored.</param>
        public async Task ObjectRestored(ActiveDirectoryObjectType objectType)
        {
            await PostClientSideCustomEvent("object_restored", objectType.ToString());
        }

        /// <summary>
        /// Posts an analytics event for an object's history being viewed.
        /// </summary>
        /// <param name="objectType">The type of the Active Directory object whose history was viewed.</param>
        public async Task ObjectHistoryViewed(ActiveDirectoryObjectType objectType)
        {
            await PostClientSideCustomEvent("object_history_viewed", objectType.ToString());
        }

        /// <summary>
        /// Posts an analytics event for an object's password being reset.
        /// </summary>
        /// <param name="objectType">The type of the Active Directory object whose password was reset.</param>
        public async Task ObjectPasswordReset(ActiveDirectoryObjectType objectType)
        {
            await PostClientSideCustomEvent("object_password_reset", objectType.ToString());
        }

        /// <summary>
        /// Posts an analytics event for the finalization of the application installation.
        /// </summary>
        public async Task InstallationFinalized()
        {
            await PostClientSideCustomEvent("installation_finalized");
        }

        /// <summary>
        /// Posts an analytics event for a new rule being created.
        /// </summary>
        /// <param name="ruleName">The name of the rule that was created.</param>
        public async Task RuleCreated(string ruleName)
        {
            await PostClientSideCustomEvent("rule_created", ruleName);
        }

        /// <summary>
        /// Posts an analytics event for a rule being executed.
        /// </summary>
        /// <param name="ruleName">The name of the rule that was executed.</param>
        /// <param name="success">A boolean indicating whether the rule execution was successful.</param>
        public async Task RuleExecuted(string ruleName, bool success)
        {
            await PostClientSideCustomEvent("rule_executed", new { ruleName, success });
        }

        /// <summary>
        /// Posts an analytics event for a new webhook being created.
        /// </summary>
        /// <param name="webhookName">The name of the webhook that was created.</param>
        public async Task WebhookCreated(string webhookName)
        {
            await PostClientSideCustomEvent("webhook_created", webhookName);
        }

        /// <summary>
        /// Posts an analytics event for a webhook being executed.
        /// </summary>
        /// <param name="webhookName">The name of the webhook that was executed.</param>
        /// <param name="success">A boolean indicating whether the webhook execution was successful.</param>
        public async Task WebhookExecuted(string webhookName, bool success)
        {
            await PostClientSideCustomEvent("webhook_executed", new { webhookName, success });
        }

        /// <summary>
        /// Posts an analytics event for a new access request being created.
        /// </summary>
        /// <param name="requestDetails">Details about the access request.</param>
        public async Task AccessRequestCreated(string requestDetails)
        {
            await PostClientSideCustomEvent("access_request_created", requestDetails);
        }

        /// <summary>
        /// Posts an analytics event for an access request being approved.
        /// </summary>
        /// <param name="requestDetails">Details about the approved access request.</param>
        public async Task AccessRequestApproved(string requestDetails)
        {
            await PostClientSideCustomEvent("access_request_approved", requestDetails);
        }

        /// <summary>
        /// Posts an analytics event for an access request being denied.
        /// </summary>
        /// <param name="requestDetails">Details about the denied access request.</param>
        public async Task AccessRequestDenied(string requestDetails)
        {
            await PostClientSideCustomEvent("access_request_denied", requestDetails);
        }

        /// <summary>
        /// Logs the execution of a specified rule by sending a custom event to the server.
        /// </summary>
        /// <param name="ruleName">The name of the rule that was executed. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task RuleExecuted(string ruleName)
        {
            await PostServerSideCustomEvent("rule_executed", ruleName);
        }

        /// <summary>
        /// Sends a server-side event indicating that an update attempt has been made.
        /// </summary>
        /// <remarks>This method posts a custom event named "update_attempted" to the server.  It is
        /// intended to be used for tracking or logging update attempts.</remarks>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task UpdateAttempted(string version)
        {
            await PostClientSideCustomEvent("update_attempted", version);
        }

        /// <summary>
        /// Posts an analytics event for a notification being dismissed.
        /// </summary>
        /// <param name="notificationType">The type of the notification that was dismissed.</param>
        public async Task NotificationDismissed(string notificationType)
        {
            await PostClientSideCustomEvent("notification_dismissed", notificationType);
        }

        /// <summary>
        /// Posts a custom analytics event to the underlying JavaScript interop.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="data">Optional data payload for the event (will be JSON serialized).</param>
        public async Task PostClientSideCustomEvent(string eventName, object? data = null)
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
                Loggers.SystemLogger.Warning(ex, "Error attempting to send Google Analytics event {EventName}", eventName);
            }
        }

        /// <summary>
        /// Posts a custom analytics event server-side to Google Analytics 4 using the Measurement Protocol.
        /// Requires measurementId and apiSecret (apiSecret recommended to be stored securely, e.g. environment variable).
        /// If measurementId is null this will attempt to use ProductionGA4Property as measurementId.
        /// </summary>
        /// <param name="eventName">Event name (GA4-safe).</param>
        /// <param name="data">Optional event params (must be primitives, nested objects become JSON strings).</param>
        /// <param name="measurementId">GA4 Measurement ID (e.g., G-XXXX). Optional if ProductionGA4Property available.</param>
        /// <param name="apiSecret">GA4 API secret. If null the method will look for environment variable "GA4_API_SECRET".</param>
        public async Task PostServerSideCustomEvent(string eventName, object? data = null, string? measurementId = null, string? apiSecret = null)
        {
            try
            {
                if (_httpClient == null)
                {
                    Loggers.SystemLogger.Warning("Analytics: HttpClient not configured; server-side analytics disabled.");
                    return;
                }

                measurementId ??= ProductionGA4Property;
                apiSecret ??= Environment.GetEnvironmentVariable("GA4_API_SECRET");

                if (string.IsNullOrWhiteSpace(measurementId) || string.IsNullOrWhiteSpace(apiSecret))
                {
                    Loggers.SystemLogger.Warning("Analytics: Missing GA4 measurementId or apiSecret. Server-side event '{EventName}' not sent.", eventName);
                    return;
                }

                try
                {
                    // GA4 requires a client_id or user_id. Use a server-generated client_id here.
                    // For better tracking across sessions, supply a stable user_id if you have an opaque non-PII identifier.
                    var clientId = Guid.NewGuid().ToString(); // or use a stable id per-user if available (no PII)

                    object eventParams;
                    if (data == null)
                    {
                        eventParams = new { };
                    }
                    else if (data is string s)
                    {
                        eventParams = new { label = s };
                    }
                    else
                    {
                        // if complex, include as nested object; GA4 params expects primitives, so convert complex objects to a JSON string under 'payload'
                        var token = data.GetType().IsPrimitive ? data : (object)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(data))!;
                        // Prefer to send simple key/value pairs; for anything else send as 'payload' string
                        eventParams = token is Newtonsoft.Json.Linq.JObject jo ? jo : new { payload = JsonConvert.SerializeObject(data) };
                    }

                    var payload = new
                    {
                        client_id = clientId,
                        events = new[]
                        {
                        new
                        {
                            name = eventName,
                            @params = eventParams
                        }
                    }
                    };

                    var json = JsonConvert.SerializeObject(payload);
                    var url = $"https://www.google-analytics.com/mp/collect?measurement_id={measurementId}&api_secret={apiSecret}";

                    using var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await _httpClient.PostAsync(url, content);
                    if (!response.IsSuccessStatusCode)
                    {
                        var respText = await response.Content.ReadAsStringAsync();
                        Loggers.SystemLogger.Warning("Analytics: GA4 server-side event '{EventName}' returned status {Status}. Response: {Resp}", eventName, response.StatusCode, respText);
                    }
                    else
                    {
                        Loggers.SystemLogger.Debug("Analytics: GA4 server-side event '{EventName}' sent successfully.", eventName);
                    }
                }
                catch (Exception ex)
                {
                    Loggers.SystemLogger.Warning(ex, "Analytics: Error sending server-side GA4 event {EventName}", eventName);
                }
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Warning(ex, "Error attempting to send Google Analytics event {EventName}", eventName);
            }
        }

        /// <summary>
        /// Posts an analytics event when a directory template is created.
        /// </summary>
        /// <param name="templateName">Template name.</param>
        public async Task DirectoryTemplateCreated(string templateName)
        {
            await PostClientSideCustomEvent("directory_template_created", templateName);
        }

        /// <summary>
        /// Posts an analytics event when a directory template is edited.
        /// </summary>
        /// <param name="templateName">Template name.</param>
        public async Task DirectoryTemplateEdited(string templateName)
        {
            await PostClientSideCustomEvent("directory_template_edited", templateName);
        }

        /// <summary>
        /// Posts an analytics event when permission mappings change.
        /// </summary>
        /// <param name="mappingName">Mapping identifier/type.</param>
        /// <param name="action">Action performed: created/updated/deleted.</param>
        public async Task PermissionMappingChanged(int numberOfDelegates)
        {
            await PostClientSideCustomEvent("permission_mapping_changed", numberOfDelegates);
        }

      /// <summary>
      /// Triggers a client-side event indicating that a permission mapping has been created.
      /// </summary>
      /// <remarks>This method sends a custom event named "permission_mapping_created" to the client side, 
      /// including the specified number of delegates as part of the event data.</remarks>
      /// <param name="numberOfDelegates">The number of delegates involved in the permission mapping. Must be a non-negative integer.</param>
      /// <returns></returns>
        public async Task PermissionMappingCreated(int numberOfDelegates)
        {
            await PostClientSideCustomEvent("permission_mapping_created", numberOfDelegates);
        }

        /// <summary>
        /// Triggers a client-side event indicating that a permission mapping has been deleted.
        /// </summary>
        /// <remarks>This method sends a custom event named "permission_mapping_deleted" to the client
        /// side. It is typically used to notify the client of changes in permission mappings.</remarks>
        /// <returns></returns>
        public async Task PermissionMappingDeleted()
        {
            await PostClientSideCustomEvent("permission_mapping_deleted");
        }

        /// <summary>
        /// Sends a custom event indicating that a permission delegate has been created.
        /// </summary>
        /// <remarks>This method triggers an asynchronous operation to post a client-side event named
        /// "permission_delegate_created".</remarks>
        /// <param name="objectType">The type of Active Directory object for which the permission delegate was created.</param>
        /// <returns></returns>
        public async Task PermissionDelegateCreated(ActiveDirectoryObjectType objectType)
        {
            await PostClientSideCustomEvent("permission_delegate_created", objectType);
        }

        /// <summary>
        /// Notifies that the permission delegate has changed for a specified Active Directory object type.
        /// </summary>
        /// <remarks>This method triggers a client-side event to handle changes in permission
        /// delegation.</remarks>
        /// <param name="objectType">The type of Active Directory object for which the permission delegate has changed.</param>
        /// <returns></returns>
        public async Task PermissionDelegateChanged(ActiveDirectoryObjectType objectType)
        {
            await PostClientSideCustomEvent("permission_delegate_changed", objectType);
        }

        /// <summary>
        /// Triggers an event indicating that the permission access level has changed.
        /// </summary>
        /// <remarks>This method asynchronously posts a custom event named
        /// "permission_access_level_changed" to the client side.</remarks>
        /// <returns></returns>
        public async Task PermissionAccessLevelChanged()
        {
            await PostClientSideCustomEvent("permission_access_level_changed");
        }

        /// <summary>
        /// Sends a custom event indicating that a permission access level has been created.
        /// </summary>
        /// <remarks>This method triggers a client-side event named "permission_access_level_created". It
        /// is intended to notify listeners about the creation of a new permission access level.</remarks>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task PermissionAccessLevelCreated()
        {
            await PostClientSideCustomEvent("permission_access_level_created");
        }
        /// <summary>
        /// Posts an analytics event when a dashboard widget is added.
        /// </summary>
        /// <param name="widgetName">Widget name.</param>
        public async Task DashboardWidgetAdded(string widgetName)
        {
            await PostClientSideCustomEvent("dashboard_widget_added", widgetName);
        }

        /// <summary>
        /// Posts an analytics event when a dashboard widget is removed.
        /// </summary>
        /// <param name="widgetName">Widget name.</param>
        public async Task DashboardWidgetRemoved(string widgetName)
        {
            await PostClientSideCustomEvent("dashboard_widget_removed", widgetName);
        }

        /// <summary>
        /// Posts an analytics event when an API token is created.
        /// </summary>
        public async Task ApiTokenCreated()
        {
            await PostClientSideCustomEvent("api_token_created");
        }

        /// <summary>
        /// Posts an analytics event when an API token is revoked.
        /// </summary>
        /// <param name="tokenName">Token friendly name.</param>
        /// <param name="revokedBy">Who revoked the token (use non-identifying label).</param>
        public async Task ApiTokenRevoked()
        {
            await PostClientSideCustomEvent("api_token_revoked");
        }
    }
}
