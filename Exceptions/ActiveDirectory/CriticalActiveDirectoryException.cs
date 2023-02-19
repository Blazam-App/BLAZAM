using BLAZAM.Common.Data.ActiveDirectory;
using BLAZAM.Server.Exceptions;

namespace BLAZAM.Server.Errors.ActiveDirectory
{
    public class CriticalActiveDirectoryException:AppException
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
