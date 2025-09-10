

namespace BLAZAM.Notifications.Services
{
    public class WebhookVerificationException : AppException
    {
        public WebhookVerificationException() : base() { }
        public WebhookVerificationException(string message) : base(message) { }
        public WebhookVerificationException(string message, Exception inner) : base(message, inner) { }

    }
}