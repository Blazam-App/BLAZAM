namespace BLAZAM.ActiveDirectory.Exceptions
{
    public class PasswordPolicyViolationException : AppException
    {
        public PasswordPolicyViolationException() : base("Password does not meet complexity requirements, has been used before, or is too new.")
        {
        }

        public PasswordPolicyViolationException(string? message) : base(message)
        {
        }

        public PasswordPolicyViolationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

    }
}