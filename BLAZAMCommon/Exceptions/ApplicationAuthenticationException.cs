using System.Runtime.Serialization;

namespace BLAZAM.Common.Exceptions
{
    [Serializable]
    public class AuthenticationException : ApplicationException
    {
        public AuthenticationException()
        {
        }

        public AuthenticationException(string? message) : base(message)
        {
        }

        public AuthenticationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected AuthenticationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}