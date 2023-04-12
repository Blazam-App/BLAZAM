using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data.Database;
using BLAZAM.Common.Data.Services;
using BLAZAM.Database.Context;
using BLAZAM.Notifications.Services;
using BLAZAM.Session.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.ActiveDirectory
{
    public class ActiveDirectoryContextFactory : IActiveDirectoryContextFactory
    {
        AppDatabaseFactory factory;
        IApplicationUserStateService userStateService;
        WmiFactory wmiFactory;
        IEncryptionService encryptionService;
        INotificationPublisher notificationPublisher;
        ActiveDirectoryContext activeDirectoryContextSeed;

        public ActiveDirectoryContextFactory(AppDatabaseFactory factory, IApplicationUserStateService userStateService, IEncryptionService encryptionService, INotificationPublisher notificationPublisher)
        {
            this.factory = factory;
            this.userStateService = userStateService;
            //TODO Create WMI Factory
            //this.wmiFactory = wmiFactory;
            this.encryptionService = encryptionService;
            this.notificationPublisher = notificationPublisher;
            activeDirectoryContextSeed = new ActiveDirectoryContext(factory, userStateService, wmiFactory, encryptionService, notificationPublisher);
        }

        public IActiveDirectoryContext CreateActiveDirectoryContext(ICurrentUserStateService? currentUserStateService = null)
        {
            var context = new ActiveDirectoryContext(activeDirectoryContextSeed);
            context.CurrentUser = currentUserStateService?.State;
            return context;

        }
    }
}
