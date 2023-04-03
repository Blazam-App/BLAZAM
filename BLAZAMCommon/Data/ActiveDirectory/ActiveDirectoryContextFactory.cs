using BLAZAM.Common.Data.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data.Database;
using BLAZAM.Common.Data.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Common.Data.ActiveDirectory
{
    public class ActiveDirectoryContextFactory : IActiveDirectoryContextFactory
    {
        AppDatabaseFactory factory;
        IApplicationUserStateService userStateService;
        WmiFactoryService wmiFactory;
        IEncryptionService encryptionService;
        INotificationPublisher notificationPublisher;
        ActiveDirectoryContext activeDirectoryContextSeed;

        public ActiveDirectoryContextFactory(AppDatabaseFactory factory, IApplicationUserStateService userStateService, WmiFactoryService wmiFactory, IEncryptionService encryptionService, INotificationPublisher notificationPublisher)
        {
            this.factory = factory;
            this.userStateService = userStateService;
            this.wmiFactory = wmiFactory;
            this.encryptionService = encryptionService;
            this.notificationPublisher = notificationPublisher;
            this.activeDirectoryContextSeed = new ActiveDirectoryContext(factory, userStateService, wmiFactory, encryptionService, notificationPublisher);
        }

        public IActiveDirectoryContext CreateActiveDirectoryContext(ICurrentUserStateService? currentUserStateService = null)
        {
            var context = new ActiveDirectoryContext(activeDirectoryContextSeed);
            context.CurrentUser = currentUserStateService?.State;
            return context;

        }
    }
}
