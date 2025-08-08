using System.Text.Json;
using ApplicationNews;
using BLAZAM.Database.Context;
using BLAZAM.Database.Services;
using BLAZAM.Jobs;
using BLAZAM.Localization;
using BLAZAM.Logger;
using BLAZAM.Session.Interfaces;
using Microsoft.Extensions.Localization;

namespace BLAZAM.Services.Background
{
    [AutoStartBackgroundService(true)]
    public class ApplicationNewsService : DatabaseBackgroundServiceBase, IApplicationNewsService
    {
        private readonly HttpClient _httpClient;
        private readonly HttpClient _secondaryHttpClient;
        private bool _pollCompleted = false;
        private List<NewsItem> _allNewsItems = new();
        private List<NewsItem> _activeNewsItems => _allNewsItems.Where(x => x.DeletedAt == null && x.Published == true && (x.ScheduledAt == null || x.ScheduledAt < DateTime.Now) && (x.ExpiresAt == null || x.ExpiresAt > DateTime.Now)).ToList();
        public AppDelegate OnNewItemsAvailable { get; set; }

        public ApplicationNewsService(IAppDatabaseFactory dbFactory, IStringLocalizer<AppLocalization> appLocalization) : base(dbFactory, appLocalization)
        {
            Interval = TimeSpan.FromMinutes(15);

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://blazam.org/api/"),
                Timeout = TimeSpan.FromSeconds(60)
            };
            _secondaryHttpClient = new HttpClient
            {
                BaseAddress = new Uri("https://blazam-news.azurewebsites.net/api/"),
                Timeout = TimeSpan.FromSeconds(60)
            };
        }


        protected override void Execute(object? state = null)
        {
            Job newsCollectionJob = new Job(AppLocalization[Lang.Fetch_News]);
            newsCollectionJob.StopOnFailedStep = true;
            JobStep collectStep = new JobStep(AppLocalization[Lang.Excute], async (step) =>
            {
                try
                {
                    _pollCompleted = false;
                    try
                    {
                        return await GetNewsAsync(_httpClient);
                    }
                    catch
                    {
                        return await GetNewsAsync(_secondaryHttpClient);
                    }
                }
                catch (Exception ex)
                {
                    Loggers.SystemLogger.Warning(ex, "Unable to contact application news API {@URI}", _httpClient.BaseAddress);
                }
                return false;
            });
            newsCollectionJob.AddStep(collectStep);
            newsCollectionJob.Run();
        }

        private async Task<bool> GetNewsAsync(HttpClient httpClient)
        {
            var apiResponse = await httpClient.GetAsync("newsItems");
            if (apiResponse != null && apiResponse.IsSuccessStatusCode)
            {
                var content = await apiResponse.Content.ReadAsStringAsync();
                var allNewsItems = JsonSerializer.Deserialize<List<NewsItem>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (allNewsItems != null)
                {
                    _allNewsItems = allNewsItems;
                    _pollCompleted = true;

                    OnNewItemsAvailable?.Invoke();

                }
                return true;
            }
            throw new AppException("News API did not return a successful response.");
        }

        public List<NewsItem> GetUnreadNewsItems(IApplicationUserState user)
        {
            try
            {
                var activeItems = _activeNewsItems;
                var unreadItems = new List<NewsItem>();
                // If the user has no read items, return all active items
                if (user?.ReadNewsItems != null)
                {
                    foreach (var item in activeItems)
                    {
                        bool isRead = user.ReadNewsItems.Any(x => x.NewsItemId == item.Id);
                        bool isUpdated = user.ReadNewsItems.Any(r => r.NewsItemId == item.Id && r.NewsItemUpdatedAt < item.UpdatedAt);

                        if (!isRead || isUpdated)
                        {
                            unreadItems.Add(item);
                        }
                    }
                }

                // Clean up stale read items that are no longer active
                if (_pollCompleted && user?.ReadNewsItems != null)
                {
                    var staleItems = user.ReadNewsItems
                        .Where(x => x.NewsItemId < 100000000000 && !activeItems.Any(a => a.Id == x.NewsItemId))
                        .ToList();

                    if (staleItems.Count > 0)
                    {
                        foreach (var x in staleItems)
                        {
                            user.ReadNewsItems.Remove(x);
                        }
                        user.SaveReadNewsItems();
                    }
                }

                return unreadItems;
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex, "Error while trying to get unread news items for user.");
                return new();
            }
        }


        public List<NewsItem> GetReadNewsItems(IApplicationUserState user)
        {
            try
            {
                if (user != null)
                {
                    var activeItems = _activeNewsItems;
                    if (user.ReadNewsItems != null)
                    {
                        var readItems = activeItems.Where(x => user.ReadNewsItems.Any(r => r.NewsItemId == x.Id && r.NewsItemUpdatedAt >= x.UpdatedAt)).ToList();

                        return readItems;
                    }

                    return new();
                }
                return new List<NewsItem>();

            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex, "Error while trying to get read news items for user.");
                return new();
            }
        }
        protected override void Dispose(bool disposing)
        {

            if (disposing)
            {
                _httpClient.Dispose();
                _secondaryHttpClient.Dispose();
            }



            base.Dispose(disposing);
        }

    }
}
