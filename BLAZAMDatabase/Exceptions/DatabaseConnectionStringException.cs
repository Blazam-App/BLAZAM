namespace BLAZAM.Database.Exceptions
{

    public class DatabaseConnectionStringException : AppException
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

    }
}