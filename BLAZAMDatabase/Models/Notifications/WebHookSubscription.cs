using System.ComponentModel.DataAnnotations.Schema;
using BLAZAM.Helpers;

namespace BLAZAM.Database.Models.Notifications
{
    public enum WebHookMethod
    {
        GET,
        POST
    }

    public enum WebHookSignature
    {
        None,
        HMAC,
        Asymmetric
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
        /// <summary>
        /// Currently not used, but could be used for future per OU mapping of webhook subscriptions
        /// </summary>
        public string? OU { get; set; }
        public List<WebHookAttempt> WebHookAttempts { get; set; } = new();

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
        public WebHookMethod WebHookMethod { get; set; } = WebHookMethod.POST;

        /// <summary>
        /// The signature to type to include in the message to verify the
        /// authenticity of the message by the receiver
        /// </summary>
        public WebHookSignature WebHookSignature { get; set; }

        /// <summary>
        /// The authorization type to use. None, basic, or bearer.
        /// </summary>
        public WebHookAuthorization WebHookAuthorization { get; set; }
        /// <summary>
        /// The destination authorization token in encrypted form
        /// </summary>
        public string? AuthorizationToken { get; set; }

        /// <summary>
        /// The authenticity HMAC key for this webhook connection
        /// in encrypted form
        /// </summary>
        public string? HmacKey { get; set; }

        public string? PrivateKey { get; set; }
        public string? PublicKey { get; set; }

        //public bool Block { get; set; } = false;
        [NotMapped]
        public bool IsValid
        {
            get
            {
                var valid = ((WebHookAuthorization == WebHookAuthorization.None || !AuthorizationToken.IsNullOrEmpty()) &&
                        NotificationTypes.Count > 0 &&
                        !URL.IsNullOrEmpty());
                return valid;
            }
        }
    }
}
