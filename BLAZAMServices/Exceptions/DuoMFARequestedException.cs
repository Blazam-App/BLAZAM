using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Services.Exceptions
{
    public class DuoMFARequestedException : MFARequestedException
    {
        public DuoMFARequestedException(LoginRequest state) : base(state)
        {
        }

        public DuoMFARequestedException(LoginRequest state, string? message) : base(state, message)
        {
        }

        public DuoMFARequestedException(LoginRequest state, string? message, Exception? innerException) : base(state, message, innerException)
        {
        }
    }
}
