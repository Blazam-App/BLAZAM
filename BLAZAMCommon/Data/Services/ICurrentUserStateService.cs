using BLAZAM.Common.Data.Services;

namespace BLAZAM.Common.Data.Services
{
    public interface ICurrentUserStateService:IDisposable
    {
        IApplicationUserState State { get; set; }
        string Username { get; }
    }
}