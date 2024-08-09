using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data.Services;
using BLAZAM.Database.Context;
using BLAZAM.Notifications.Services;
using BLAZAM.Session.Interfaces;

namespace BLAZAM.ActiveDirectory
{
    public class ActiveDirectoryContextFactory : IActiveDirectoryContextFactory
    {
        //IAppDatabaseFactory _factory;
        //IApplicationUserStateService _userStateService;
        //IEncryptionService _encryptionService;
        //INotificationPublisher _notificationPublisher;
        ActiveDirectoryContext activeDirectoryContextSeed;

        public ActiveDirectoryContextFactory(IAppDatabaseFactory factory, IApplicationUserStateService userStateService, IEncryptionService encryptionService, INotificationPublisher notificationPublisher)
        {
           // _factory = factory;
           //_userStateService = userStateService;
           // _encryptionService = encryptionService;
           // _notificationPublisher = notificationPublisher;
            activeDirectoryContextSeed = new ActiveDirectoryContext(factory, userStateService, encryptionService, notificationPublisher);
        }

        public IActiveDirectoryContext CreateActiveDirectoryContext(ICurrentUserStateService? currentUserStateService = null)
        {
            var context = new ActiveDirectoryContext(activeDirectoryContextSeed);
            context.ConnectAsync();
            context.CurrentUser = currentUserStateService?.State;
            return context;

        }
    }
}
