using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using BLAZAM.Session.Interfaces;

namespace BLAZAM.ActiveDirectory
{
    /// <summary>
    /// An <see cref="IActiveDirectoryContext"/> intended for each web user connection
    /// </summary>
    public class ScopedActiveDirectoryContext : IDisposable
    {
        private IActiveDirectoryContextFactory _contextFactory;

        public IActiveDirectoryContext Context { get; }

        /// <summary>
        /// Creates a new <see cref="IActiveDirectoryContext"/> scoped to the current Blazor session
        /// </summary>
        /// <param name="contextFactory">The system based context factory</param>
        /// <param name="currentUser">The current user</param>
        public ScopedActiveDirectoryContext(IActiveDirectoryContextFactory contextFactory, ICurrentUserStateService currentUser)
        {
            _contextFactory = contextFactory;
            Context = _contextFactory.CreateActiveDirectoryContext(currentUser);
            ApplicationStatistics.AddADContext();

        }
        /// <summary>
        /// Removes all created <see cref="IActiveDirectoryContext"/>'s created by this user's factory
        /// </summary>
        public void Dispose()
        {
            ApplicationStatistics.RemoveADContext();

            Context.Dispose();
        }
    }
}
