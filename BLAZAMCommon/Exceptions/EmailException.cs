using System.Runtime.Serialization;

namespace BLAZAM.Server.Data.Services.Email
{
    [Serializable]
    public class EmailException : ApplicationException
    {
        public EmailException()
        {
        }

        public EmailException(string? message) : base(message)
        {
        }

        public EmailException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected EmailException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}