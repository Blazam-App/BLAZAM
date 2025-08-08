using System.Runtime.Serialization;

namespace BLAZAM.Update.Exceptions
{
    [Serializable]
    public class ApplicationUpdateException : AppException
    {
        public ApplicationUpdateException()
        {
        }

        public ApplicationUpdateException(string? message) : base(message)
        {
        }

        public ApplicationUpdateException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected ApplicationUpdateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}