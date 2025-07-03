using BLAZAM.ActiveDirectory.Data;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data.Services;
using BLAZAM.Database.Context;
using BLAZAM.Notifications.Services;

namespace BLAZAM.ActiveDirectory
{
    public class ActiveDirectoryContextFactory : IActiveDirectoryContextFactory
    {

        private readonly ActiveDirectoryContext activeDirectoryContextSeed;


        public ActiveDirectoryContextFactory(IAppDatabaseFactory factory, IEncryptionService encryptionService, INotificationPublisher notificationPublisher)
        {

            activeDirectoryContextSeed = new ActiveDirectoryContext(factory, encryptionService, notificationPublisher);
        }

        public IActiveDirectoryContext CreateActiveDirectoryContext(ActiveDirectoryUserState? currentUserState = null)
        {
            var context = new ActiveDirectoryContext(activeDirectoryContextSeed);
            //_ = context.ConnectAsync();
            context.CurrentUser=currentUserState;
            return context;

        }





    }
}
