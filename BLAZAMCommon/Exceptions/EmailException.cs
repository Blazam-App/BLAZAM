using System.Runtime.Serialization;

namespace BLAZAM.Common.Exceptions
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