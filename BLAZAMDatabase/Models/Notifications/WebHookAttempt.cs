using System.Net;

namespace BLAZAM.Database.Models.Notifications
{
    public class WebHookAttempt : AppDbSetBase
    {
        public Guid MessageGuid { get; set; }
        public DateTime LastAttemptTimestamp { get; set; }
        public DateTime EventTimestamp { get; set; }
        public WebHookSubscription WebHookSubscription { get; set; }
        public string EventType { get; set; }
        public int WebHookSubscriptionId { get; set; }
        public string Uri { get; set; }
        public bool Delivered { get; set; }
        public string Body { get; set; }
        public string? Signature { get; set; }
        public int RetryCount { get; set; }
        public HttpStatusCode ResponseCode { get; set; }
        public string? ResponseHeaders { get; set; }
        public string? ResponseMessage { get; set; }
    }
}
