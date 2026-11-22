using System;
using System.Collections.Generic;
using System.Text;

namespace BLAZAM.Global.Data
{
    public class ExpiringMemoryCache
    {
        private static object _operationLock = new object();
        private TimeSpan _timeToLive { get; set; }
        private object? _cachedData{ get; set; }
        
        private DateTime _expirationTime { get; set; }
        
        public ExpiringMemoryCache()
        {
            lock (_operationLock)
            {
                _timeToLive = TimeSpan.MinValue;
                _expirationTime = DateTime.Now;
            }
        }
        public ExpiringMemoryCache(object obj, TimeSpan? ttl = null)
        {
            lock (_operationLock)
            {
                if (!ttl.HasValue)
                {
                    ttl = TimeSpan.FromMinutes(5);
                }
            }
            SetValue(obj, ttl.Value);
        }

        public void SetValue(object obj, TimeSpan ttl)
        {
            lock (_operationLock)
            {
                _timeToLive = ttl;
                _cachedData = obj;
                _expirationTime = DateTime.Now + ttl;
            }
        }

        public T? GetValue<T>()
        {
            if (DateTime.Now > _expirationTime)
            {
                return default;
            }
            return (T?)_cachedData;
        }

    }
}
