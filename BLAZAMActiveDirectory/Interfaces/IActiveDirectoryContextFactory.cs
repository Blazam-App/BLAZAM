using BLAZAM.ActiveDirectory.Data;

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
        /// <param name="currentUserState"></param>
        /// <returns></returns>
        IActiveDirectoryContext CreateActiveDirectoryContext(ActiveDirectoryUserState? currentUserState = null);
    }
}