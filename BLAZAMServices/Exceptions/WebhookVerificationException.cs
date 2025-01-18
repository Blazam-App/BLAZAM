
using BLAZAM.Common.Exceptions;

namespace BLAZAM.Notifications.Services
{
    [Serializable]
    public class WebhookVerificationException : AppException
    {
        public WebhookVerificationException() : base() { }
        public WebhookVerificationException(string message) : base(message) { }
        public WebhookVerificationException(string message, Exception inner) : base(message, inner) { }

        protected WebhookVerificationException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}