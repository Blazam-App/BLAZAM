using System.Runtime.Serialization;

namespace BLAZAM.Global.Exceptions
{
    [Serializable]
    public class LockedOutUserException : AppException
    {
        public LockedOutUserException()
        {
        }

        public LockedOutUserException(string? message) : base(message)
        {
        }

        public LockedOutUserException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected LockedOutUserException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}