using System.Runtime.Serialization;

namespace BLAZAM.Server.Data.Services.Update
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