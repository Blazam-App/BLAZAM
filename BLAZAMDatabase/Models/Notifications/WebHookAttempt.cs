using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Database.Models.Notifications
{
    public class WebHookAttempt:AppDbSetBase
    {
        public Guid MessageGuid { get; set; }
        public Guid AttemptGuid { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime EventTimestamp { get; set; }
        public WebHookSubscription WebHookSubscription { get;set;}
        public int WebHookSubscriptionId {get;set;}
        public string Uri { get; set; }
        public bool Delivered { get; set; }
        public string Body { get; set; }
        public string? Signature { get; set; }
        public int RetryCount { get; set; }
        public HttpStatusCode RepsonseCode { get; set; }
        public string? ResponseMessage { get; set; }
    }
}
