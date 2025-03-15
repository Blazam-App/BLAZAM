
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Exceptions;

namespace BLAZAM.ActiveDirectory.Exceptions
{
    public class CriticalActiveDirectoryException : AppException
    {
        public IActiveDirectoryContext Context { get; }
        public override string Message { get; }
        public CriticalActiveDirectoryException(IActiveDirectoryContext context, string message)
        {
            Context = context;
            Message = message;
        }
    }
}
