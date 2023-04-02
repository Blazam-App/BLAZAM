using BLAZAM.Common.Data.ActiveDirectory;

namespace BLAZAM.Common.Exceptions
{
    public class CriticalActiveDirectoryException: ApplicationException
    {
        public ActiveDirectoryContext Context { get;}
        public override string Message { get;}
        public CriticalActiveDirectoryException(ActiveDirectoryContext context,string message)
        {
            Context = context;
            Message = message;
        }
    }
}
