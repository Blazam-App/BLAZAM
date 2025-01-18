using System.Runtime.Serialization;

namespace BLAZAM.ActiveDirectory.Exceptions
{
    [Serializable]
    public class WmiConnectionException : ApplicationException
    {
        public WmiConnectionException()
        {
        }

        public WmiConnectionException(string? message) : base(message)
        {
        }

        public WmiConnectionException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected WmiConnectionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}