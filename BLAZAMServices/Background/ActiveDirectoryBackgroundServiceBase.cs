using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Context;

namespace BLAZAM.Services.Background
{
    public class ActiveDirectoryBackgroundServiceBase : BackgroundServiceBase
    {
        protected readonly IActiveDirectoryContextFactory activeDirectoryContextFactory;

        public ActiveDirectoryBackgroundServiceBase(IActiveDirectoryContextFactory activeDirectoryContextFactory, IAppDatabaseFactory dbFactory) : base(dbFactory)
        {
            this.activeDirectoryContextFactory = activeDirectoryContextFactory;
        }
    }
}
