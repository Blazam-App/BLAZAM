using BLAZAM.Database.Models.User;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BLAZAM.Database.Models.Notifications
{
    public enum WebHookMethod
    {
        GET,
        POST
    }
    public enum WebHookAuthorization
    {
        None,
        Basic,
        Bearer
    }
    public class WebHookSubscription : RecoverableAppDbSetBase
    {
        /// <summary>
        /// The notification types this subscription should trigger on
        /// </summary>
        public List<SubscriptionWebHookType> NotificationTypes { get; set; } = new();
        //public string OU { get; set; }

        /// <summary>
        /// Whether or not to ignore SSL Certificate verification when sending the webhook
        /// </summary>
        public bool IgnoreSSLVerification { get; set; }

        /// <summary>
        /// The full URL with protocol appended
        /// </summary>
        /// <remarks>
        /// eg: https://blazam.org/consumer
        /// </remarks>
        public string URL { get; set; }
        /// <summary>
        /// The HTTP method to use
        /// </summary>
        public WebHookMethod WebHookMethod { get; set; }

        /// <summary>
        /// The authorization type to use. None, basic, or bearer.
        /// </summary>
        public WebHookAuthorization WebHookAuthorization { get;set;}
        /// <summary>
        /// The destination authorization token in encrypted form
        /// </summary>
        public string? AuthorizationToken { get; set; }

        //public bool Block { get; set; } = false;
    }
}
