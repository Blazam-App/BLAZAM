using System.Runtime.Serialization;

namespace BLAZAM
{
    [Serializable]
    internal class DeniedLoginException : ApplicationException
    {
        public DeniedLoginException()
        {
        }

        public DeniedLoginException(string? message) : base(message)
        {
        }

        public DeniedLoginException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected DeniedLoginException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}