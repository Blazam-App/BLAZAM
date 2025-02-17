using BLAZAM.Database.Context;
using BLAZAM.Services.Background;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Database.Services
{
    public class DatabaseBackgroundServiceBase:BackgroundServiceBase
    {
        protected readonly IAppDatabaseFactory dbFactory;

        public DatabaseBackgroundServiceBase(IAppDatabaseFactory dbFactory)
        {
            this.dbFactory = dbFactory;

        }
    }
}
