using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Models.Notifications;
using BLAZAM.Logger;
using BLAZAM.Session.Interfaces;
using MudBlazor.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Notifications.Services
{
    public class WebHookPublisher
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public WebHookPublisher(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }


        public async Task PublishWebhook(WebHookSubscription subscription, IDirectoryEntryAdapter source, NotificationType notificationType, IApplicationUserState? actor = null, IDirectoryEntryAdapter? target = null)
        {
            Dictionary<string, object?> payload = new()
                {
                    { "timestamp", DateTime.UtcNow.ToString() },
                  
                    { "type", notificationType.ToString() }
                };
            Dictionary<string, object?> data = new()
            {
                  { "actor", actor?.Username }, // Use ?. to handle null actor
                    { "object", source?.CanonicalName }, // Use ?. to handle null target
                    { "objectDN", source?.DN }, // Use ?. to handle null target
                    { "objectType", source?.ObjectType.ToString()}, // Use ?. to handle null target
            };
            payload.Add("data", data);

            var httpClient = _httpClientFactory.CreateClient();

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(subscription.URL),
                Method = subscription.WebHookMethod == WebHookMethod.GET ? HttpMethod.Get : HttpMethod.Post,
                Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };

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
                response.EnsureSuccessStatusCode(); // Throw an exception if not successful
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log the error)
                Loggers.SystemLogger.Error("Webhook failed {Error}", ex);
            }
        }
    }
}
