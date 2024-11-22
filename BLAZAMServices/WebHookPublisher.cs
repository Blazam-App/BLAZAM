using Azure.Core.Pipeline;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using BLAZAM.Database.Models.Notifications;
using BLAZAM.Helpers;
using BLAZAM.Logger;
using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Configuration;
using MudBlazor.Extensions;
using System;
using Polly.Extensions.Http;
using Polly;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BLAZAM.Database.Context;
using System.Data.Entity;

namespace BLAZAM.Notifications.Services
{
    public class WebHookPublisher : IDisposable
    {
        internal static readonly UTF8Encoding SafeUTF8Encoding = new UTF8Encoding(false, true);
        internal const string UNBRANDED_ID_HEADER_KEY = "webhook-id";
        internal const string UNBRANDED_SIGNATURE_HEADER_KEY = "webhook-signature";
        internal const string UNBRANDED_TIMESTAMP_HEADER_KEY = "webhook-timestamp";
        internal const string UNBRANDED_ATTEMPT_TIMESTAMP_HEADER_KEY = "webhook-attempt-timestamp";

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
                        Parallel.ForEachAsync(undeliveredWebhooks, async (attempt, cancel) =>
                        {
                            var attemptId = Guid.NewGuid();
                            var webHookAttempt = new WebHookAttempt()
                            {
                                Body = attempt.Body,
                                RetryCount = attempt.RetryCount++,
                                AttemptGuid = attemptId,
                                MessageGuid = attempt.MessageGuid,
                                WebHookSubscriptionId = attempt.WebHookSubscriptionId,
                                Timestamp = DateTime.UtcNow,
                                EventTimestamp = attempt.EventTimestamp,
                                Signature = attempt.Signature
                            };
                            await SendWebHook(attempt.WebHookSubscription, attempt.MessageGuid, attemptId, attempt.EventTimestamp.ToString(), attempt.Signature, attempt.Body);

                        });
                    }catch(Exception ex)
                    {
                        Loggers.SystemLogger.Error("Unexpected error retrying webhook {Error}", ex);
                    }
                    var rand = new Random();

                    await Task.Delay(60000 + rand.Next(-10000, 10000));
                }
            }
        }
        public async Task PublishWebhook(WebHookSubscription subscription,
            IDirectoryEntryAdapter source,
            NotificationType notificationType,
            IApplicationUserState? actor = null,
            IDirectoryEntryAdapter? target = null)
        {
            var msgId = Guid.NewGuid();
            var timestamp = DateTime.UtcNow.ToString();



            string? signature = null;

            Dictionary<string, object?> payload = new()
                {
                    { "timestamp", timestamp},

                    { "type", source.ObjectType.ToString().ToLower()+"."+notificationType.ToString().ToLower() }
                };
            var attemptId = Guid.NewGuid();
            Dictionary<string, object?> data = new()
            {
                  { "id", msgId }, // Use ?. to handle null actor
                  { "attemptId", attemptId }, // Use ?. to handle null actor
                  { "actor", actor?.Username }, // Use ?. to handle null actor
                    { "entry", source?.CanonicalName }, // Use ?. to handle null target
                    { "entryOU", source?.OU }, // Use ?. to handle null target
                    { "entryDN", source?.DN }, // Use ?. to handle null target
                    { "entryType", source?.ObjectType.ToString()}, // Use ?. to handle null target
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
                if (subscription.HmacKey.IsNullOrEmpty()) throw new ApplicationException("HMAC Key not supplied to subscription set to use it.");
                var key = subscription.HmacKey.Decrypt<string>();
                if (key.StartsWith(prefix))
                {
                    key = key.Substring(prefix.Length);
                }
                var bytekey = Convert.FromBase64String(key);
                signature = Sign(bytekey, msgId.ToString(), DateTime.UtcNow, payloadString);

            }
            var webHookAttempt = new WebHookAttempt()
            {
                Body = payloadString,
                AttemptGuid = attemptId,
                MessageGuid = msgId,
                WebHookSubscriptionId = subscription.Id,
                Timestamp = DateTime.UtcNow,
                EventTimestamp = DateTime.Parse(timestamp),
                Signature = signature
            };
            using var context = await _appDatabaseFactory.CreateDbContextAsync();
            context.WebHookAttempts.Add(webHookAttempt);
            await context.SaveChangesAsync();
            await SendWebHook(subscription, msgId, attemptId, timestamp, signature, payloadString);
        }

        private async Task SendWebHook(WebHookSubscription subscription, Guid msgId, Guid attemptId, string timestamp, string? signature, string payloadString)
        {
            var httpClient = CreateAPIClient();

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(subscription.URL),
                Method = subscription.WebHookMethod == WebHookMethod.GET ? HttpMethod.Get : HttpMethod.Post,
                Content = new StringContent(payloadString, Encoding.UTF8, "application/json")

            };
            request.Headers.Add(UNBRANDED_ID_HEADER_KEY, msgId.ToString());
            request.Headers.Add(UNBRANDED_TIMESTAMP_HEADER_KEY, timestamp);
            request.Headers.Add(UNBRANDED_ATTEMPT_TIMESTAMP_HEADER_KEY, DateTime.UtcNow.ToString());
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

                var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    using var context = await _appDatabaseFactory.CreateDbContextAsync();
                    var thisAttempt = await context.WebHookAttempts.FirstOrDefaultAsync(a => a.AttemptGuid == attemptId);
                    thisAttempt.Delivered = true;
                    thisAttempt.RepsonseCode = response.StatusCode;
                    thisAttempt.ResponseMessage = response.ReasonPhrase;
                    await context.SaveChangesAsync();
                }
                else
                {
                    Loggers.SystemLogger.Information("Webhook failed {Subscription}", subscription);
                    using var context = await _appDatabaseFactory.CreateDbContextAsync();
                    var thisAttempt = await context.WebHookAttempts.FirstOrDefaultAsync(a => a.AttemptGuid == attemptId);
                    thisAttempt.Delivered = false;
                    thisAttempt.RepsonseCode = response.StatusCode;
                    thisAttempt.ResponseMessage = response.ReasonPhrase;
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log the error)
                Loggers.SystemLogger.Error("Unexpected Webhook error occurred {Error}", ex);

            }
        }

        private HttpClient CreateAPIClient()
        {
            return _httpClientFactory.CreateClient(HttpClientNames.WebHookHttpClientName);
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
