using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Services.Exceptions
{
    public class GoogleMFARequestedException : MFARequestedException
    {
        public GoogleMFARequestedException(LoginRequest state) : base(state)
        {
        }

        public GoogleMFARequestedException(LoginRequest state, string? message) : base(state, message)
        {
        }

        public GoogleMFARequestedException(LoginRequest state, string? message, Exception? innerException) : base(state, message, innerException)
        {
        }
    }
}
