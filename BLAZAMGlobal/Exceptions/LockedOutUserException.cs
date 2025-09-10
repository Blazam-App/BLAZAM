namespace BLAZAM.Global.Exceptions
{
    public class LockedOutUserException : AppException
    {
        public LockedOutUserException()
        {
        }

        public LockedOutUserException(string? message) : base(message)
        {
        }

        public LockedOutUserException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

    }
}