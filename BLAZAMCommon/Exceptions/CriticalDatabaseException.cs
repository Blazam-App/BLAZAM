
using BLAZAM.Common.Data.Database;

namespace BLAZAM.Server.Errors.Database
{
    public class CriticalDatabaseException: ApplicationException
    {
        public IDatabaseContext Context { get;}
        public override string Message { get;}
        public CriticalDatabaseException(IDatabaseContext context,string message)
        {
            Context = context;
            Message = message;
        }
    }
}
