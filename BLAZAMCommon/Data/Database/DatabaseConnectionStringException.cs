using System.Runtime.Serialization;

namespace BLAZAM.Common.Data.Database
{
    [Serializable]
    internal class DatabaseConnectionStringException : ApplicationException
    {
        public DatabaseConnectionStringException()
        {
        }

        public DatabaseConnectionStringException(string? message) : base(message)
        {
        }

        public DatabaseConnectionStringException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected DatabaseConnectionStringException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}