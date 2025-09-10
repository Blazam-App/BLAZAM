namespace BLAZAM.ActiveDirectory.Exceptions
{
    public class WmiConnectionException : AppException
    {
        public WmiConnectionException()
        {
        }

        public WmiConnectionException(string? message) : base(message)
        {
        }

        public WmiConnectionException(string? message, Exception? innerException) : base(message, innerException)
        {
        }


    }
}