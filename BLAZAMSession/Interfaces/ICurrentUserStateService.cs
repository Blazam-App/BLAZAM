namespace BLAZAM.Session.Interfaces
{
    public interface ICurrentUserStateService : IDisposable
    {
        IApplicationUserState State { get; set; }
        string Username { get; }
    }
}