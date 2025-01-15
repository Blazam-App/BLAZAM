using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
