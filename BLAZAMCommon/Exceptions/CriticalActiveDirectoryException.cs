using BLAZAM.Common.Data.ActiveDirectory;

namespace BLAZAM.Server.Errors.ActiveDirectory
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
