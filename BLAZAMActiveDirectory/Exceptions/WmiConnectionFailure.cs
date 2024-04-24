using System.Runtime.Serialization;

namespace BLAZAM.ActiveDirectory.Exceptions
{
    [Serializable]
    internal class WmiConnectionFailure : Exception
    {
        public WmiConnectionFailure()
        {
        }

        public WmiConnectionFailure(string? message) : base(message)
        {
        }

        public WmiConnectionFailure(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected WmiConnectionFailure(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}