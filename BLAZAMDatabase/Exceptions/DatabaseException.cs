namespace BLAZAM.Database.Exceptions
{

    public class DatabaseException : AppException
    {
        public DatabaseException()
        {
        }

        public DatabaseException(string? message) : base(message)
        {
        }

        public DatabaseException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

    }
}