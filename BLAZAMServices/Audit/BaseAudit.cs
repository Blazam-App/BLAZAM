using BLAZAM.Database.Context;

namespace BLAZAM.Services.Audit
{
    public class BaseAudit
    {
        protected IAppDatabaseFactory Factory { get; set; }

        public BaseAudit(IAppDatabaseFactory factory)
        {
            Factory = factory;
        }
    }
}