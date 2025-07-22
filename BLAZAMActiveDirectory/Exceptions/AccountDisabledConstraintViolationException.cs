
using BLAZAM.Common.Exceptions;
using System.Runtime.Serialization;

namespace BLAZAM.ActiveDirectory.Adapters
{
    [Serializable]
    public class AccountDisabledConstraintViolationException : AppException
    {
        public AccountDisabledConstraintViolationException():base("The account is disabled and there is a policy preventing this action.")
        {
        }

        public AccountDisabledConstraintViolationException(string? message) : base(message)
        {
        }

        public AccountDisabledConstraintViolationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
        protected AccountDisabledConstraintViolationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}