using BLAZAM.Common.Data.Services;
using BLAZAM.Session.Interfaces;

namespace BLAZAM.ActiveDirectory.Interfaces
{
    /// <summary>
    /// A factory for creating Active Directory connections
    /// </summary>
    public interface IActiveDirectoryContextFactory
    {
        /// <summary>
        /// Creates a new Active Directory connection context. If a user state is provided, the connection will be created
        /// for only that web user.
        /// </summary>
        /// <param name="currentUserStateService"></param>
        /// <returns></returns>
        IActiveDirectoryContext CreateActiveDirectoryContext(ICurrentUserStateService? currentUserStateService = null);
    }
}