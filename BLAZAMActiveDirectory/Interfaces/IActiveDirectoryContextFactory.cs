using BLAZAM.Common.Data.Services;
using BLAZAM.Session.Interfaces;

namespace BLAZAM.ActiveDirectory.Interfaces
{
    public interface IActiveDirectoryContextFactory
    {
        IActiveDirectoryContext CreateActiveDirectoryContext(ICurrentUserStateService? currentUserStateService = null);
    }
}