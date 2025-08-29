namespace BLAZAM.Services.Exceptions
{
    public class MFARequestedException : AppException
    {
        public LoginRequest LoginRequest { get; set; }
        public MFARequestedException(LoginRequest state)
        {
            LoginRequest = state;
        }

        public MFARequestedException(LoginRequest state, string? message) : base(message)
        {
            LoginRequest = state;

        }

        public MFARequestedException(LoginRequest state, string? message, Exception? innerException) : base(message, innerException)
        {
            LoginRequest = state;
        }


    }
}