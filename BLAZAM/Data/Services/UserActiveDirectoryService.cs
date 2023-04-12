using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data.Services;
using BLAZAM.Session.Interfaces;

namespace BLAZAM.Server.Data.Services
{
    public class UserActiveDirectoryService
    {
        private IActiveDirectoryContextFactory _contextFactory;

        public IActiveDirectoryContext Context { get; }

        public UserActiveDirectoryService(IActiveDirectoryContextFactory contextFactory, ICurrentUserStateService currentUser)
        {
            _contextFactory = contextFactory;
            Context = _contextFactory.CreateActiveDirectoryContext(currentUser);
        }
    }
}
