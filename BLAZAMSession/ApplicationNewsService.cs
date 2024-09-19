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
        private HttpClient _secondaryHttpClient;
        private Timer? _pollingTimer;
        private bool _pollCompleted = false;
        private List<NewsItem> _allNewsItems = new List<NewsItem>();
        private List<NewsItem> activeNewsItems => _allNewsItems.Where(x => x.DeletedAt == null && x.Published == true && (x.ScheduledAt == null || x.ScheduledAt < DateTime.Now) && (x.ExpiresAt == null || x.ExpiresAt > DateTime.Now)).ToList();
        public AppEvent OnNewItemsAvailable { get; set; }
        public ApplicationNewsService()
        {
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
            try
            {
                var activeItems = activeNewsItems;
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

                var activeItems = activeNewsItems;
                if (user.ReadNewsItems != null)
                {
                    var readItems = activeItems.Where(x => user.ReadNewsItems.Any(r => r.NewsItemId == x.Id && r.NewsItemUpdatedAt >= x.UpdatedAt)).ToList();

                    return readItems;
                }

                return new();

            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error("Error while trying to get read news items for user. {@Error}", ex);
                return new();
            }
        }
        public void Dispose()
        {
            _httpClient.Dispose();
            _pollingTimer?.Dispose();
        }
    }
}
