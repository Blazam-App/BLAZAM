using BLAZAM.Common.Data;
using BLAZAM.Database.Context;
using BLAZAM.Logger;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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

        public async void ObjectMoved(ActiveDirectoryObjectType objectType)
        {
            await PostCustomEvent("object_moved",objectType.ToString());
        }

        public async void ObjectCreated(ActiveDirectoryObjectType objectType)
        {
            await PostCustomEvent("object_created",objectType.ToString());
        }

        public async void ObjectDeleted(ActiveDirectoryObjectType objectType)
        {
            await PostCustomEvent("object_deleted", objectType.ToString());
        }

        public async void ObjectRenamed(ActiveDirectoryObjectType objectType)
        {
            await PostCustomEvent("object_renamed", objectType.ToString());
        }

        public async void ObjectEnabled(ActiveDirectoryObjectType objectType)
        {
            await PostCustomEvent("object_enabled", objectType.ToString());
        }

        public async void ObjectDisabled(ActiveDirectoryObjectType objectType)
        {
            await PostCustomEvent("object_disabled", objectType.ToString());
        }

        public async void ObjectUnlocked(ActiveDirectoryObjectType objectType)
        {
            await PostCustomEvent("object_unlocked", objectType.ToString());
        }

        public async void ObjectAssigned(ActiveDirectoryObjectType objectType)
        {
            await PostCustomEvent("object_assigned", objectType.ToString());
        }

        public async void ObjectUnassigned(ActiveDirectoryObjectType objectType)
        {
            await PostCustomEvent("object_unassigned", objectType.ToString());
        }

        public async void ObjectRestored(ActiveDirectoryObjectType objectType)
        {
            await PostCustomEvent("object_restored", objectType.ToString());
        }

        public async void ObjectHistoryViewed(ActiveDirectoryObjectType objectType)
        {
            await PostCustomEvent("object_history_viewed", objectType.ToString());
        }

        public async void ObjectPasswordReset(ActiveDirectoryObjectType objectType)
        {
            await PostCustomEvent("object_password_reset", objectType.ToString());
        }

        public async Task PostCustomEvent(string eventName,object? data=null)
        {
            try
            {
                if (data == null)
                {
                    await _jsRuntime.InvokeVoidAsync("customAnalyticsEvent", new object[] { eventName });
                }
                else
                {
                    await _jsRuntime.InvokeVoidAsync("customAnalyticsEvent", new object[] { eventName, JsonConvert.SerializeObject(data) });

                }
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Warning("Error attepmting to send Google Analytics event {EventName}{Error}", eventName, ex);
            }
        }
    }
}
