using System.Runtime.Serialization;

namespace BLAZAM.Common.Exceptions
{
    [Serializable]
    public class ApplicationUpdateCanceledException : ApplicationException
    {
        public ApplicationUpdateCanceledException()
        {
        }

        public ApplicationUpdateCanceledException(string? message) : base(message)
        {
        }

        public ApplicationUpdateCanceledException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected ApplicationUpdateCanceledException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}