using ApplicationNews;
using BLAZAM.Database.Context;
using BLAZAM.Database.Services;
using BLAZAM.Jobs;
using BLAZAM.Localization;
using BLAZAM.Logger;
using BLAZAM.Session.Interfaces;
using Microsoft.Extensions.Localization;
using System.Text.Json;

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


        protected override void Execute(object? obj = null)
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
                        var apiResponse = await _httpClient.GetAsync("newsItems");
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
                    }
                    catch
                    {
                        var apiResponse = await _secondaryHttpClient.GetAsync("newsItems");
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
                    }
                }
                catch (Exception ex)
                {
                    Loggers.SystemLogger.Warning("Unable to contact application news API {@URI}{@Error}", _httpClient.BaseAddress, ex);
                }
                return false;
            });
            newsCollectionJob.AddStep(collectStep);
            newsCollectionJob.Run();
        }
        public List<NewsItem> GetUnreadNewsItems(IApplicationUserState user)
        {
            try
            {
                var activeItems = _activeNewsItems;
                var unreadItems = new List<NewsItem>();
                foreach (var item in activeItems)
                {
                    if (user?.ReadNewsItems != null)
                    {
                        if (!user.ReadNewsItems.Any(x => x.NewsItemId == item.Id))
                            unreadItems.Add(item);
                        if (user.ReadNewsItems.Any(r => r.NewsItemId == item.Id && r.NewsItemUpdatedAt < item.UpdatedAt))
                            unreadItems.Add(item);



                    }
                }
                // var unreadItems = activeItems.Where(x => user.ReadNewsItems?.Any(r=>r.NewsItemId==x.Id)==false||user.ReadNewsItems?.Any(r=>r.NewsItemId==x.Id&& r.NewsItemUpdatedAt<x.UpdatedAt)==false).ToList();
                if (_pollCompleted && user.ReadNewsItems != null)
                {
                    var staleItems = user.ReadNewsItems.Where(x => x.NewsItemId < 100000000000 && !activeItems.Any(a => a.Id == x.NewsItemId)).ToList();
                    if (staleItems.Count > 0)
                    {
                        staleItems.ForEach(x =>
                        {
                            user.ReadNewsItems.Remove(x);
                        });
                        user.SaveReadNewsItems();
                    }
                }
                return unreadItems;
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error("Error while trying to get unread news items for user. {@Error}", ex);
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
                Loggers.SystemLogger.Error("Error while trying to get read news items for user. {@Error}", ex);
                return new();
            }
        }
        public override void Dispose()
        {
            _httpClient.Dispose();
            _secondaryHttpClient.Dispose();
        }
    }
}
