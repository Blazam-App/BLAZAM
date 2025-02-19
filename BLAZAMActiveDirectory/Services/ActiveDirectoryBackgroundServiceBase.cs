using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Context;
using BLAZAM.Database.Services;

namespace BLAZAM.ActiveDirectory.Services
{
    public class ActiveDirectoryBackgroundServiceBase : DatabaseBackgroundServiceBase
    {
        protected readonly IActiveDirectoryContextFactory activeDirectoryContextFactory;

        public ActiveDirectoryBackgroundServiceBase(IActiveDirectoryContextFactory activeDirectoryContextFactory, IAppDatabaseFactory dbFactory) : base(dbFactory)
        {
            this.activeDirectoryContextFactory = activeDirectoryContextFactory;
        }
    }
}
