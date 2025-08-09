using BLAZAM.Database.Context;
using BLAZAM.Localization;
using BLAZAM.Services.Background;
using Microsoft.Extensions.Localization;

namespace BLAZAM.Database.Services
{
    public class DatabaseBackgroundServiceBase : BackgroundServiceBase
    {
        protected readonly IAppDatabaseFactory dbFactory;

        public DatabaseBackgroundServiceBase(IAppDatabaseFactory dbFactory, IStringLocalizer<AppLocalization> appLocalization) : base(appLocalization)
        {
            this.dbFactory = dbFactory;

        }
    }
}
