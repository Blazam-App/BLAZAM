using ApplicationNews;

namespace BLAZAM.Session.Interfaces
{
    public interface IApplicationNewsService : IDisposable
    {
        AppDelegate OnNewItemsAvailable { get; set; }

        List<NewsItem> GetUnreadNewsItems(IApplicationUserState user);
        List<NewsItem> GetReadNewsItems(IApplicationUserState user);
    }
}