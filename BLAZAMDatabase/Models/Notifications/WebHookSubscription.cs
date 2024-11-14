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
        Basic,
        Bearer
    }
    public class WebHookSubscription : RecoverableAppDbSetBase
    {
        public List<SubscriptionNotificationType> NotificationTypes { get; set; } = new();
        public string OU { get; set; }
        public bool IgnoreSSLVerification { get; set; }
        public string URL { get; set; }
        public WebHookMethod WebHookMethod { get; set; }
        public WebHookAuthorization? WebHookAuthorization { get;set;}
        public string? AuthorizationToken { get; set; }

        public bool Block { get; set; } = false;
    }
}
