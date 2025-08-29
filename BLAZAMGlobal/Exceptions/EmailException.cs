namespace BLAZAM.Global.Exceptions
{
    public class EmailException : AppException
    {
        public EmailException()
        {
        }

        public EmailException(string? message) : base(message)
        {
        }

        public EmailException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

    }
}