namespace BLAZAM.Global.Exceptions
{
    public class DeniedLoginException : AppException
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

    }
}