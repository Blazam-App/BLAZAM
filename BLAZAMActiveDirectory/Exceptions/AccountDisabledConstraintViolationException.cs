namespace BLAZAM.ActiveDirectory.Adapters
{
    public class AccountDisabledConstraintViolationException : AppException
    {
        public AccountDisabledConstraintViolationException() : base("The account is disabled and there is a policy preventing this action.")
        {
        }

        public AccountDisabledConstraintViolationException(string? message) : base(message)
        {
        }

        public AccountDisabledConstraintViolationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

    }
}