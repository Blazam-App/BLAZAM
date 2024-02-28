using ApplicationNews;
using BLAZAM.Database.Context;
using BLAZAM.Logger;
using BLAZAM.Session.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BLAZAM.Session
{
    public class ApplicationNewsService : IApplicationNewsService
    {
        private HttpClient _httpClient;
        private Timer? _pollingTimer;
        private List<NewsItem> _allNewsItems = new List<NewsItem>();
        private List<NewsItem> activeNewsItems => _allNewsItems.Where(x=>x.DeletedAt==null && x.Published==true && (x.ScheduledAt==null||x.ScheduledAt<DateTime.Now)&&(x.ExpiresAt==null||x.ExpiresAt>DateTime.Now)).ToList();
        public AppEvent OnNewItemsAvailable { get; set; }
        public ApplicationNewsService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://blazam-news.azurewebsites.net/api/"),
                Timeout = TimeSpan.FromSeconds(60)
            };
            _pollingTimer = new Timer(Tick, null, 10, 1000 * 60 * 15);
           // GetAllNewsItems();
        }

        private async void Tick(object? state)
        {
            await GetAllNewsItems();
        }

        private async Task GetAllNewsItems()
        {
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
                        OnNewItemsAvailable?.Invoke();
                    }
                }
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Warning("Unable to contact application news API {@URI}{@Error}", _httpClient.BaseAddress, ex);
            }
        }
        public List<NewsItem> GetUnreadNewsItems(IApplicationUserState user)
        {
            var activeItems = activeNewsItems;
            var unreadItems = activeItems.Where(x => !user.ReadNewsItems.Any(r=>r.NewsItemId==x.Id)||user.ReadNewsItems.Any(r=>r.NewsItemId==x.Id&& r.NewsItemUpdatedAt<x.UpdatedAt)).ToList();
            var staleItems = user.ReadNewsItems.Where(x => x.NewsItemId<100000000000 && !activeItems.Any(a => a.Id == x.NewsItemId)).ToList();
            if (staleItems.Count > 0)
            {
                staleItems.ForEach(x => {
                    user.ReadNewsItems.Remove(x);
                });
                user.SaveUserSettings();
            }
            return unreadItems;

        }


        public List<NewsItem> GetReadNewsItems(IApplicationUserState user)
        {
            var activeItems = activeNewsItems;
            var readItems = activeItems.Where(x => user.ReadNewsItems.Any(r => r.NewsItemId == x.Id && r.NewsItemUpdatedAt>=x.UpdatedAt)).ToList();
            
            return readItems;

        }
        public void Dispose()
        {
            _httpClient.Dispose();
            _pollingTimer?.Dispose();
        }
    }
}
