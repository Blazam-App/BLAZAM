namespace BLAZAM.Global.Exceptions
{
    public class AuthenticationException : AppException
    {
        public AuthenticationException()
        {
        }

        public AuthenticationException(string? message) : base(message)
        {
        }

        public AuthenticationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }


    }
}