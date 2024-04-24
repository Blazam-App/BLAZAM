using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data.Services;
using BLAZAM.Session.Interfaces;

namespace BLAZAM.ActiveDirectory
{
    /// <summary>
    /// An <see cref="IActiveDirectoryContext"/> intended for each web user connection
    /// </summary>
    public class ScopedActiveDirectoryContext
    {
        private IActiveDirectoryContextFactory _contextFactory;

        public IActiveDirectoryContext Context { get; }

        public ScopedActiveDirectoryContext(IActiveDirectoryContextFactory contextFactory, ICurrentUserStateService currentUser)
        {
            _contextFactory = contextFactory;
            Context = _contextFactory.CreateActiveDirectoryContext(currentUser);
        }
    }
}
