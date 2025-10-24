namespace BLAZAM.Services.Exceptions
{
    public class GoogleMFARequestedException : AppException
    {
        public LoginRequest LoginRequest { get; set; }
        public GoogleMFARequestedException(LoginRequest state)
        {
            LoginRequest = state;
        }

        public GoogleMFARequestedException(LoginRequest state, string? message) : base(message)
        {
            LoginRequest = state;

        }

        public GoogleMFARequestedException(LoginRequest state, string? message, Exception? innerException) : base(message, innerException)
        {
            LoginRequest = state;
        }


    }
}