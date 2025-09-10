namespace BLAZAM.Update.Exceptions
{

    public class ApplicationUpdateCanceledException : AppException
    {
        public ApplicationUpdateCanceledException()
        {
        }

        public ApplicationUpdateCanceledException(string? message) : base(message)
        {
        }

        public ApplicationUpdateCanceledException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

    }
}