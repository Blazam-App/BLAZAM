namespace BLAZAM.Update.Exceptions
{
    public class ApplicationUpdateException : AppException
    {
        public ApplicationUpdateException()
        {
        }

        public ApplicationUpdateException(string? message) : base(message)
        {
        }

        public ApplicationUpdateException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

    }
}