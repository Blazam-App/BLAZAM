using BLAZAM.Database.Context;

namespace BLAZAM.Database.Exceptions
{
    public class CriticalDatabaseException : AppException
    {
        public IDatabaseContext Context { get; }
        public override string Message { get; }
        public CriticalDatabaseException(IDatabaseContext context, string message)
        {
            Context = context;
            Message = message;
        }
    }
}
