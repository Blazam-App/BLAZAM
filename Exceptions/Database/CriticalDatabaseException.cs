
using BLAZAM.Common.Data.Database;

namespace BLAZAM.Server.Errors.Database
{
    public class CriticalDatabaseException:Exception
    {
        public DatabaseContext Context { get;}
        public override string Message { get;}
        public CriticalDatabaseException(DatabaseContext context,string message)
        {
            Context = context;
            Message = message;
        }
    }
}
