using BLAZAM.ActiveDirectory.Data;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data.Services;
using BLAZAM.Notifications.Services;

namespace BLAZAM.ActiveDirectory
{
    public class ActiveDirectoryContextFactory : IActiveDirectoryContextFactory
    {

        protected ActiveDirectoryContext activeDirectoryContextSeed;


        public ActiveDirectoryContextFactory(IAppDatabaseFactory dbFactory, IEncryptionService encryptionService, INotificationPublisher notificationPublisher)
        {

            activeDirectoryContextSeed = new ActiveDirectoryContext(dbFactory, encryptionService, notificationPublisher);
        }

        public IActiveDirectoryContext CreateActiveDirectoryContext(ActiveDirectoryUserState? currentUserState = null)
        {
            var context = new ActiveDirectoryContext(activeDirectoryContextSeed);
            context.CurrentUser = currentUserState;
            return context;
        }





    }
}
