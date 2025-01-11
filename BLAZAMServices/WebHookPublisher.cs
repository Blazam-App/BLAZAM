using Azure;
using Azure.Core.Pipeline;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using BLAZAM.Common.Data.Database;
using BLAZAM.Database.Context;
using BLAZAM.Database.Models.Notifications;
using BLAZAM.Helpers;
using BLAZAM.Jobs;
using BLAZAM.Logger;
using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using Microsoft.Extensions.Http;
using MudBlazor.Extensions;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace BLAZAM.Notifications.Services
{
    public class WebHookPublisher : IDisposable
    {
        internal static readonly UTF8Encoding SafeUTF8Encoding = new UTF8Encoding(false, true);
        internal const string UNBRANDED_ID_HEADER_KEY = "webhook-id";
        internal const string UNBRANDED_ATTEMPT_ID_HEADER_KEY = "webhook-attempt-id";
        internal const string UNBRANDED_SIGNATURE_HEADER_KEY = "webhook-signature";
        internal const string UNBRANDED_TIMESTAMP_HEADER_KEY = "webhook-timestamp";
        internal const string UNBRANDED_ATTEMPT_TIMESTAMP_HEADER_KEY = "webhook-attempt-timestamp";
        private const string webhookDateTimeFormat = "yyyy-MM-dd'T'HH:mm:ss'Z'";

        private const int TOLERANCE_IN_SECONDS = 60 * 5;
        private static string prefix = "whsec_";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAppDatabaseFactory _appDatabaseFactory;

        private bool _running;
        public WebHookPublisher(IHttpClientFactory httpClientFactory, IAppDatabaseFactory appDatabaseFactory)
        {
            _httpClientFactory = httpClientFactory;
            _appDatabaseFactory = appDatabaseFactory;
            _ = Run();
        }
        private async Task Run()
        {
            if (!_running)
            {
                _running = true;
                while (_running)
                {
                    try
                    {
                        using var context = _appDatabaseFactory.CreateDbContext();
                        var undeliveredWebhooks = context.WebHookAttempts
                            .Include(w => w.WebHookSubscription)
                            .Where(w => w.Delivered == false &&
                            w.RetryCount < 15)
                            .ToList();
                        if (undeliveredWebhooks.Count > 0)
                        {
                            IJob webhookAttemptJob = new Job("Webhook Retry");
                            JobStep execStep = null;
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


        public async Task PublishWebhook(WebHookSubscription subscription,
            IDirectoryEntryAdapter source,
            NotificationType notificationType,
            IApplicationUserState? actor = null,
            IDirectoryEntryAdapter? target = null)
        {
            IJob webhookAttemptJob = new Job("Publish Webhook");
            JobStep execStep = new JobStep("Execute", async (step) =>
            {
                var msgId = Guid.NewGuid();
                var eventTimestamp = DateTime.UtcNow;

                var eventType = source.ObjectType.ToString().ToLower() + "." + notificationType.ToString().ToLower();


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
                    if (subscription.HmacKey.IsNullOrEmpty())
                        throw new ApplicationException("HMAC Key not supplied to subscription set to use it.");
                    var key = subscription.HmacKey.Decrypt<string>();
                    if (key.StartsWith(prefix))
                    {
                        key = key.Substring(prefix.Length);
                    }
                    var bytekey = Convert.FromBase64String(key);
                    signature = Sign(bytekey, msgId.ToString(), DateTime.UtcNow, payloadString);

                }

                await SendWebHook(subscription, msgId, attemptId, eventTimestamp, eventType, payloadString, signature);

                return true;
            });
            webhookAttemptJob.AddStep(execStep);
            var result = await webhookAttemptJob.RunAsync();
        }

        private async Task SendWebHook(WebHookSubscription subscription, Guid msgId, Guid attemptId, DateTime eventTimestamp, string eventType, string payloadString, string? signature)
        {
            var httpClientHandler = new HttpClientHandler();
            var httpClient = CreateAPIClient(subscription.IgnoreSSLVerification);
            using var context = await _appDatabaseFactory.CreateDbContextAsync();

            var thisMessage = await context.WebHookAttempts.FirstOrDefaultAsync(a => a.MessageGuid == msgId);



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
                request.Headers.Add(UNBRANDED_SIGNATURE_HEADER_KEY, signature);

            }
            // Add authorization header if needed
            if (subscription.WebHookAuthorization == WebHookAuthorization.Basic)
            {
                // Assuming AuthorizationToken is in the format "username:password"
                var base64Auth = Convert.ToBase64String(Encoding.ASCII.GetBytes(subscription.AuthorizationToken));
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", base64Auth);
            }
            else if (subscription.WebHookAuthorization == WebHookAuthorization.Bearer)
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", subscription.AuthorizationToken);
            }

            try
            {
                if (thisMessage == null)
                {
                    var webHookAttempt = new WebHookAttempt()
                    {
                        Body = payloadString,
                        MessageGuid = msgId,
                        EventType = eventType,
                        Uri = request.RequestUri.ToString(),
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
                    thisMessage.RetryCount++;
                    thisMessage.Uri = request.RequestUri.ToString();
                }
                await context.SaveChangesAsync();


                var response = await httpClient.SendAsync(request);






                if (response.IsSuccessStatusCode)
                {
                    thisMessage.Delivered = true;
                    thisMessage.ResponseCode = response.StatusCode;
                    thisMessage.ResponseMessage = null;
                    await context.SaveChangesAsync();
                }
                else
                {
                    Loggers.SystemLogger.Information("Webhook failed {Subscription}", subscription);
                    thisMessage.Delivered = false;
                    thisMessage.ResponseCode = response.StatusCode;
                    thisMessage.ResponseMessage = null;
                    await context.SaveChangesAsync();
                }
            }
            catch (HttpRequestException ex)
            {
                if (ex.InnerException != null)
                {
                    if (thisMessage != null)
                    {
                        thisMessage.ResponseCode = HttpStatusCode.UnprocessableEntity;
                        thisMessage.ResponseMessage = ex.InnerException.Message;
                    }

                }
                else
                {
                    if (thisMessage != null)
                    {
                        thisMessage.ResponseCode = HttpStatusCode.UnprocessableEntity;
                        thisMessage.ResponseMessage = ex.Message;
                    }
                }
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error("Unexpected Webhook error occurred {Error}", ex);
            }
        }

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
        //public void Verify(string payload, WebHeaderCollection headers)
        //{
        //    string msgId = headers.Get(UNBRANDED_ID_HEADER_KEY);
        //    string msgSignature = headers.Get(UNBRANDED_SIGNATURE_HEADER_KEY);
        //    string msgTimestamp = headers.Get(UNBRANDED_TIMESTAMP_HEADER_KEY);
        //    if (String.IsNullOrEmpty(msgId) || String.IsNullOrEmpty(msgSignature) || String.IsNullOrEmpty(msgTimestamp))
        //    {
        //        throw new WebhookVerificationException("Missing Required Headers");
        //    }

        //    var timestamp = Webhook.VerifyTimestamp(msgTimestamp);

        //    var signature = this.Sign(msgId, timestamp, payload);
        //    var expectedSignature = signature.Split(',')[1];

        //    var passedSignatures = msgSignature.Split(' ');
        //    foreach (string versionedSignature in passedSignatures)
        //    {
        //        var parts = versionedSignature.Split(',');
        //        if (parts.Length < 2)
        //        {
        //            throw new WebhookVerificationException("Invalid Signature Headers");
        //        }
        //        var version = parts[0];
        //        var passedSignature = parts[1];

        //        if (version != "v1")
        //        {
        //            continue;
        //        }
        //        if (Utils.SecureCompare(expectedSignature, passedSignature))
        //        {
        //            return;
        //        }

        //    }
        //    throw new WebhookVerificationException("No matching signature found");
        //}

        //private static DateTimeOffset VerifyTimestamp(string timestampHeader)
        //{
        //    DateTimeOffset timestamp;
        //    var now = DateTimeOffset.UtcNow;
        //    try
        //    {
        //        var timestampInt = long.Parse(timestampHeader);
        //        timestamp = DateTimeOffset.FromUnixTimeSeconds(timestampInt);
        //    }
        //    catch
        //    {
        //        throw new WebhookVerificationException("Invalid Signature Headers");
        //    }

        //    if (timestamp < (now.AddSeconds(-1 * TOLERANCE_IN_SECONDS)))
        //    {
        //        throw new WebhookVerificationException("Message timestamp too old");
        //    }
        //    if (timestamp > (now.AddSeconds(TOLERANCE_IN_SECONDS)))
        //    {
        //        throw new WebhookVerificationException("Message timestamp too new");
        //    }
        //    return timestamp;
        //}

        public string Sign(byte[] key, string msgId, DateTimeOffset timestamp, string payload)
        {
            var toSign = $"{msgId}.{timestamp.ToUnixTimeSeconds().ToString()}.{payload}";
            var toSignBytes = SafeUTF8Encoding.GetBytes(toSign);
            using (var hmac = new HMACSHA256(key))
            {
                var hash = hmac.ComputeHash(toSignBytes);
                var signature = Convert.ToBase64String(hash);
                return $"v1,{signature}";
            }
        }

        public void Dispose()
        {
            _running = false;
        }
    }
}
