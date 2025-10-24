using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data.Database;
using BLAZAM.Database.Models.Notifications;
using BLAZAM.Helpers;
using BLAZAM.Jobs;
using BLAZAM.Logger;
using BLAZAM.Session.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Cryptography;
using System.Text;


namespace BLAZAM.Services
{
    /// <summary>
    /// Manages the publishing and retrying of webhook notifications to subscribed endpoints.
    /// </summary>
    public class WebHookPublisher : IDisposable
    {
        internal static readonly UTF8Encoding SafeUTF8Encoding = new(false, true);
        internal const string UNBRANDED_ID_HEADER_KEY = "webhook-id";
        internal const string UNBRANDED_ATTEMPT_ID_HEADER_KEY = "webhook-attempt-id";
        internal const string UNBRANDED_SIGNATURE_HEADER_KEY = "webhook-signature";
        internal const string UNBRANDED_TIMESTAMP_HEADER_KEY = "webhook-timestamp";
        internal const string UNBRANDED_ATTEMPT_TIMESTAMP_HEADER_KEY = "webhook-attempt-timestamp";
        private const string webhookDateTimeFormat = "yyyy-MM-dd'T'HH:mm:ss'Z'";

        private static string prefix = "whsec_";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAppDatabaseFactory _appDatabaseFactory;

        private bool _running;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebHookPublisher"/> class.
        /// </summary>
        /// <param name="httpClientFactory">Factory for creating HTTP clients.</param>
        /// <param name="appDatabaseFactory">Factory for creating database contexts.</param>
        /// <exception cref="ArgumentNullException">Thrown if httpClientFactory or appDatabaseFactory is null.</exception>
        public WebHookPublisher(IHttpClientFactory httpClientFactory, IAppDatabaseFactory appDatabaseFactory)
        {
            ArgumentNullException.ThrowIfNull(httpClientFactory);

            ArgumentNullException.ThrowIfNull(appDatabaseFactory);


            _httpClientFactory = httpClientFactory;
            _appDatabaseFactory = appDatabaseFactory;
            _ = Run();
        }

        /// <summary>
        /// Starts the background task that periodically retries failed webhooks.
        /// </summary>
        private async Task Run()
        {
            if (!_running)
            {
                Loggers.SystemLogger.Information("WebHookPublisher.Run: Background webhook retry task started.");
                _running = true;
                while (_running)
                {
                    try
                    {
                        if (ApplicationInfo.installationCompleted)
                        {
                            await RetryFailedWebhooks();
                        }
                    }
                    catch (Exception ex)
                    {
                        Loggers.SystemLogger.Error("Unexpected error retrying webhook {Error}", ex);
                    }
                    var rand = new Random();

                    await Task.Delay(600000 + rand.Next(-10000, 10000));
                }
            }
        }

        /// <summary>
        /// Retrieves and attempts to resend webhooks that previously failed delivery.
        /// </summary>
        private async Task RetryFailedWebhooks()
        {
            using var context = await _appDatabaseFactory.CreateDbContextAsync();
            var undeliveredWebhooks = await context.WebHookAttempts
                                                                    .Include(w => w.WebHookSubscription)
                                                                    .Where(w => w.Delivered == false &&
                                                                    w.RetryCount < 15)
                                                                    .ToListAsync();
            Loggers.SystemLogger.Information("WebHookPublisher.RetryFailedWebhooks: Found {Count} undelivered webhooks to retry.", undeliveredWebhooks.Count);

            if (undeliveredWebhooks.Count > 0)
            {
                IJob webhookAttemptJob = new Job("Webhook Retry");
                JobStep? execStep = null;
                if (_appDatabaseFactory.DatabaseType == DatabaseType.SQLite)
                {

                    foreach (var attempt in undeliveredWebhooks)
                    {
                        execStep = new JobStep("Execute " + attempt.WebHookSubscription.URL, async (step) =>
                        {
                            var attemptId = Guid.NewGuid();

                            await SendWebHook(attempt.WebHookSubscription, attempt.MessageGuid, attemptId, attempt.EventTimestamp, attempt.Body, attempt.EventType, attempt.Signature);
                            return true;

                        });
                        webhookAttemptJob.AddStep(execStep);
                    }
                }
                else
                {

                    execStep = new JobStep("Multi-threaded execute of " + undeliveredWebhooks.Count + " retries", (step) =>
                    {
                        Parallel.ForEachAsync(undeliveredWebhooks, async (attempt, cancel) =>
                        {
                            var attemptId = Guid.NewGuid();

                            await SendWebHook(attempt.WebHookSubscription, attempt.MessageGuid, attemptId, attempt.EventTimestamp, attempt.Body, attempt.EventType, attempt.Signature);

                        });
                        return true;
                    });
                    webhookAttemptJob.AddStep(execStep);

                }
                await webhookAttemptJob.RunAsync();
                Loggers.SystemLogger.Information("WebHookPublisher.RetryFailedWebhooks: Webhook retry job {JobStatus}. Passed: {PassedCount}, Failed: {FailedCount}", webhookAttemptJob.Result, webhookAttemptJob.PassedSteps.Count, webhookAttemptJob.FailedSteps.Count);
            }
        }

        /// <summary>
        /// Constructs and sends a new webhook notification based on a triggering event.
        /// </summary>
        /// <param name="subscription">The webhook subscription details.</param>
        /// <param name="source">The directory entry that is the source of the event.</param>
        /// <param name="notificationType">The type of notification.</param>
        /// <param name="actor">The user state of the actor who initiated the event, if applicable.</param>
        /// <param name="target">An optional target directory entry related to the event.</param>
        public async Task PublishWebhook(WebHookSubscription subscription,
            IDirectoryEntryAdapter source,
            NotificationType notificationType,
            IApplicationUserState? actor = null,
            IDirectoryEntryAdapter? target = null)
        {
            var eventType = source.ObjectType.ToString().ToLower() + "." + notificationType.ToString().ToLower();
            Loggers.SystemLogger.Information("WebHookPublisher.PublishWebhook: Publishing webhook for subscription ID {SubscriptionId}, EventType {EventType}, Source: {SourceDN}", subscription.Id, eventType, source?.DN ?? "N/A");

            IJob webhookAttemptJob = new Job("Publish Webhook");
            JobStep execStep = new("Execute", async (step) =>
            {
                var msgId = Guid.NewGuid();
                var eventTimestamp = DateTime.UtcNow;
                string? signature = null;

                Dictionary<string, object?> payload = new()
                {
                    { "timestamp", eventTimestamp.ToString(webhookDateTimeFormat)},
                    { "type", eventType}
                };
                var attemptId = Guid.NewGuid();
                Dictionary<string, object?> data = new()
                {
                  { "id", msgId },
                  { "actor", actor?.Username }, // Use ?. to handle null actor
                    { "entry", source?.CanonicalName }, // Use ?. to handle null source
                    { "entryOU", source?.OU }, // Use ?. to handle null source
                    { "entryDN", source?.DN }, // Use ?. to handle null source
                    { "entryType", source?.ObjectType.ToString()}, // Use ?. to handle null source
                };
                if (target != null)
                {
                    data.Add("target", target.CanonicalName);
                    data.Add("targetOU", target.OU);
                    data.Add("targetDN", target.DN);
                    data.Add("targetType", target.ObjectType.ToString());
                }
                payload.Add("data", data);
                var payloadString = System.Text.Json.JsonSerializer.Serialize(payload);
                if (subscription.WebHookSignature == WebHookSignature.HMAC)
                {
                    if (subscription.HmacKey is null || subscription.HmacKey.IsNullOrEmpty())
                    {
                        throw new AppException("HMAC Key not supplied to subscription set to use it.");
                    }

                    var key = subscription.HmacKey.Decrypt<string>();
                    if (key != null && key.StartsWith(prefix))
                    {
                        key = key.Substring(prefix.Length);
                        var bytekey = Convert.FromBase64String(key);
                        signature = Sign(bytekey, msgId.ToString(), DateTime.UtcNow, payloadString);
                    }
                }
                await SendWebHook(subscription, msgId, attemptId, eventTimestamp, eventType, payloadString, signature);
                return true;
            });
            webhookAttemptJob.AddStep(execStep);
            var result = await webhookAttemptJob.RunAsync();
            Loggers.SystemLogger.Information("WebHookPublisher.PublishWebhook: Publish job for subscription ID {SubscriptionId} completed with status {JobStatus}.", subscription.Id, webhookAttemptJob.Result);
        }

        /// <summary>
        /// Sends the actual webhook HTTP request and handles the response, including saving attempt details.
        /// </summary>
        /// <param name="subscription">The webhook subscription.</param>
        /// <param name="msgId">The unique message ID for this webhook event.</param>
        /// <param name="attemptId">The unique ID for this specific attempt.</param>
        /// <param name="eventTimestamp">The timestamp of the original event.</param>
        /// <param name="eventType">The type of the event (e.g., "user.created").</param>
        /// <param name="payloadString">The JSON serialized payload.</param>
        /// <param name="signature">The optional signature for the payload.</param>
        private async Task SendWebHook(WebHookSubscription subscription, Guid msgId, Guid attemptId, DateTime eventTimestamp, string eventType, string payloadString, string? signature)
        {
            var httpClient = CreateAPIClient(subscription.IgnoreSSLVerification);
            using var context = await _appDatabaseFactory.CreateDbContextAsync();

            var thisMessage = await context.WebHookAttempts.FirstOrDefaultAsync(a => a.MessageGuid == msgId);

            var request = BuildHttpRequest(subscription, msgId, attemptId, eventTimestamp, payloadString, signature);

            try
            {
                thisMessage = PrepareWebHookAttempt(context, thisMessage, subscription, msgId, eventTimestamp, eventType, payloadString, signature, request.RequestUri.ToString());
                await context.SaveChangesAsync();

                var response = await httpClient.SendAsync(request);

                await HandleWebHookResponse(context, thisMessage, response, attemptId, subscription.Id, request.RequestUri);
            }
            catch (HttpRequestException ex)
            {
                await HandleHttpRequestException(context, thisMessage, ex);
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error("Unexpected Webhook error occurred {Error}", ex);
            }
        }

        private HttpRequestMessage BuildHttpRequest(WebHookSubscription subscription, Guid msgId, Guid attemptId, DateTime eventTimestamp, string payloadString, string? signature)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(subscription.URL),
                Method = subscription.WebHookMethod == WebHookMethod.GET ? HttpMethod.Get : HttpMethod.Post,
                Content = new StringContent(payloadString, Encoding.UTF8, "application/json")
            };

            request.Headers.Add(UNBRANDED_ID_HEADER_KEY, msgId.ToString());
            request.Headers.Add(UNBRANDED_ATTEMPT_ID_HEADER_KEY, attemptId.ToString());
            request.Headers.Add(UNBRANDED_TIMESTAMP_HEADER_KEY, eventTimestamp.ToString(webhookDateTimeFormat));
            request.Headers.Add(UNBRANDED_ATTEMPT_TIMESTAMP_HEADER_KEY, DateTime.UtcNow.ToString(webhookDateTimeFormat));
            if (!signature.IsNullOrEmpty())
            {
                Loggers.SystemLogger.Debug("WebHookPublisher.SendWebHook: Added signature to webhook attempt ID {AttemptId}.", attemptId);
                request.Headers.Add(UNBRANDED_SIGNATURE_HEADER_KEY, signature);
            }
            if (subscription.WebHookAuthorization == WebHookAuthorization.Basic && subscription.AuthorizationToken != null)
            {
                var base64Auth = Convert.ToBase64String(Encoding.ASCII.GetBytes(subscription.AuthorizationToken));
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", base64Auth);
            }
            else if (subscription.WebHookAuthorization == WebHookAuthorization.Bearer)
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", subscription.AuthorizationToken);
            }
            return request;
        }

        private WebHookAttempt PrepareWebHookAttempt(IDatabaseContext context, WebHookAttempt? thisMessage, WebHookSubscription subscription, Guid msgId, DateTime eventTimestamp, string eventType, string payloadString, string? signature, string uri)
        {
            if (thisMessage == null)
            {
                var webHookAttempt = new WebHookAttempt()
                {
                    Body = payloadString,
                    MessageGuid = msgId,
                    EventType = eventType,
                    Uri = uri,
                    Delivered = false,
                    WebHookSubscriptionId = subscription.Id,
                    LastAttemptTimestamp = DateTime.UtcNow,
                    EventTimestamp = eventTimestamp,
                    Signature = signature
                };
                thisMessage = webHookAttempt;
                context.WebHookAttempts.Add(thisMessage);
            }
            else
            {
                thisMessage.LastAttemptTimestamp = DateTime.UtcNow;
                thisMessage.RetryCount++;
                thisMessage.Uri = uri;
            }
            return thisMessage;
        }

        private async Task HandleWebHookResponse(IDatabaseContext context, WebHookAttempt thisMessage, HttpResponseMessage response, Guid attemptId, int subscriptionId, Uri requestUri)
        {
            if (response.IsSuccessStatusCode)
            {
                Loggers.SystemLogger.Information("WebHookPublisher.SendWebHook: Webhook attempt ID {AttemptId} sent successfully to {RequestUri}. Status: {StatusCode}", attemptId, requestUri, response.StatusCode);
                thisMessage.Delivered = true;
                thisMessage.ResponseCode = response.StatusCode;
                thisMessage.ResponseMessage = null;
            }
            else
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                thisMessage.ResponseMessage = responseContent;
                Loggers.SystemLogger.Warning("WebHookPublisher.SendWebHook: Webhook attempt ID {AttemptId} failed for subscription {SubscriptionId} to {RequestUri}. Status: {StatusCode}, Response: {ResponseMessage}", attemptId, subscriptionId, requestUri, response.StatusCode, thisMessage.ResponseMessage);
                thisMessage.Delivered = false;
                thisMessage.ResponseCode = response.StatusCode;
            }
            await context.SaveChangesAsync();
        }

        private async Task HandleHttpRequestException(IDatabaseContext context, WebHookAttempt? thisMessage, HttpRequestException ex)
        {
            if (thisMessage != null)
            {
                thisMessage.ResponseCode = HttpStatusCode.UnprocessableEntity;
                thisMessage.ResponseMessage = ex.InnerException?.Message ?? ex.Message;
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Creates an HttpClient instance, optionally configured to ignore SSL certificate validation errors.
        /// </summary>
        /// <param name="ignoreSSL">True to ignore SSL errors; false otherwise.</param>
        /// <returns>An HttpClient instance.</returns>
        private HttpClient CreateAPIClient(bool ignoreSSL)
        {
            if (!ignoreSSL)
            {
                // Use the default client if validating ssl
                return _httpClientFactory.CreateClient(HttpClientNames.WebHookHttpClientName);
            }
            else
            {

                // Use the client with validation disabled
                var client = _httpClientFactory.CreateClient(HttpClientNames.WebHookHttpClientNoSSLCheckName);

                return client;
            }
        }


        /// <summary>
        /// Generates an HMACSHA256 signature for the webhook payload.
        /// </summary>
        /// <param name="key">The secret key (Base64 decoded).</param>
        /// <param name="msgId">The message ID.</param>
        /// <param name="timestamp">The timestamp of the event.</param>
        /// <param name="payload">The JSON payload string.</param>
        /// <returns>A versioned signature string (e.g., "v1,ActualSignatureBase64").</returns>
        public string Sign(byte[] key, string msgId, DateTimeOffset timestamp, string payload)
        {
            var toSign = $"{msgId}.{timestamp.ToUnixTimeSeconds().ToString()}.{payload}";
            var toSignBytes = SafeUTF8Encoding.GetBytes(toSign);
            using var hmac = new HMACSHA256(key);
            var hash = hmac.ComputeHash(toSignBytes);
            var signature = Convert.ToBase64String(hash);
            return $"v1,{signature}";
        }

        /// <summary>
        /// Stops the background webhook retry task.
        /// </summary>
        public void Dispose()
        {
            Loggers.SystemLogger.Information("WebHookPublisher.Run: Background webhook retry task stopping.");
            _running = false;
        }
    }
}
