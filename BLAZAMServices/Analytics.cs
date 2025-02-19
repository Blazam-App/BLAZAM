using BLAZAM.Database.Context;
using BLAZAM.Logger;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace BLAZAM.Services
{
    public class Analytics
    {
        private readonly IAppDatabaseFactory _dbFactory;
        private readonly IJSRuntime _jsRuntime;

        private IDatabaseContext _databaseContext => _dbFactory.CreateDbContext();

        protected string? ProductionGA4Property
        {
            get
            {
                using var context = _databaseContext;
                return context.AppSettings.FirstOrDefault()?.AnalyticsId;
            }
        }

        public Analytics(IAppDatabaseFactory dbFactory, IJSRuntime jSRuntime)
        {
            _dbFactory = dbFactory;
            _jsRuntime = jSRuntime;
        }

        public async Task ObjectMoved(ActiveDirectoryObjectType objectType)
        {
            await PostCustomEvent("object_moved", objectType.ToString());
        }

        public async Task ObjectCreated(ActiveDirectoryObjectType objectType)
        {
            await PostCustomEvent("object_created", objectType.ToString());
        }

        public async Task ObjectDeleted(ActiveDirectoryObjectType objectType)
        {
            await PostCustomEvent("object_deleted", objectType.ToString());
        }

        public async Task ObjectRenamed(ActiveDirectoryObjectType objectType)
        {
            await PostCustomEvent("object_renamed", objectType.ToString());
        }

        public async Task ObjectEnabled(ActiveDirectoryObjectType objectType)
        {
            await PostCustomEvent("object_enabled", objectType.ToString());
        }

        public async Task ObjectDisabled(ActiveDirectoryObjectType objectType)
        {
            await PostCustomEvent("object_disabled", objectType.ToString());
        }

        public async Task ObjectUnlocked(ActiveDirectoryObjectType objectType)
        {
            await PostCustomEvent("object_unlocked", objectType.ToString());
        }

        public async Task ObjectAssigned(ActiveDirectoryObjectType objectType)
        {
            await PostCustomEvent("object_assigned", objectType.ToString());
        }

        public async Task ObjectUnassigned(ActiveDirectoryObjectType objectType)
        {
            await PostCustomEvent("object_unassigned", objectType.ToString());
        }

        public async Task ObjectRestored(ActiveDirectoryObjectType objectType)
        {
            await PostCustomEvent("object_restored", objectType.ToString());
        }

        public async Task ObjectHistoryViewed(ActiveDirectoryObjectType objectType)
        {
            await PostCustomEvent("object_history_viewed", objectType.ToString());
        }

        public async Task ObjectPasswordReset(ActiveDirectoryObjectType objectType)
        {
            await PostCustomEvent("object_password_reset", objectType.ToString());
        }

        public async Task PostCustomEvent(string eventName, object? data = null)
        {
            try
            {
                if (data == null)
                {
                    await _jsRuntime.InvokeVoidAsync("customAnalyticsEvent", new object[] { eventName });
                }
                else
                {
                    await _jsRuntime.InvokeVoidAsync("customAnalyticsEvent", new object[] { eventName, JsonConvert.SerializeObject(new object[] { data }) });

                }
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Warning("Error attepmting to send Google Analytics event {EventName}{Error}", eventName, ex);
            }
        }
    }
}
