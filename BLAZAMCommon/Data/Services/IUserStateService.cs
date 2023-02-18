
namespace BLAZAM.Common.Data.Services
{
    public interface IUserStateService
    {
        IApplicationUserState CurrentUserState { get; set; }
        string Username { get; }
    }
}