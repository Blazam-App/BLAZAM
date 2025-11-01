using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Services;
using BLAZAM.Localization;
using Microsoft.Extensions.Localization;

namespace BLAZAM.ActiveDirectory.Services
{
    public class ActiveDirectoryBackgroundServiceBase : DatabaseBackgroundServiceBase
    {
        protected readonly IActiveDirectoryContextFactory activeDirectoryContextFactory;

        public ActiveDirectoryBackgroundServiceBase(IActiveDirectoryContextFactory activeDirectoryContextFactory, IAppDatabaseFactory dbFactory, IStringLocalizer<AppLocalization> appLocalization) : base(dbFactory, appLocalization)
        {
            this.activeDirectoryContextFactory = activeDirectoryContextFactory;
        }
    }
}
