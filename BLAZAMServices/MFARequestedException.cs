using BLAZAM.Common.Data;
using Microsoft.AspNetCore.Components.Authorization;
using System.Runtime.Serialization;

namespace BLAZAM.Services
{
    [Serializable]
    internal class MFARequestedException : Exception
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

        protected MFARequestedException(LoginRequest state, SerializationInfo info, StreamingContext context) : base(info, context)
        {
            LoginRequest = state;
        }
    }
}