using BLAZAM.Common.Data.Services;

namespace BLAZAM.Common.Data.ActiveDirectory.Interfaces
{
    public interface IActiveDirectoryContextFactory
    {
        IActiveDirectoryContext CreateActiveDirectoryContext(ICurrentUserStateService currentUserStateService = null);
    }
}