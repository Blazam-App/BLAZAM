﻿using BLAZAM.Common.Data.Services;
using BLAZAM.Session.Interfaces;

namespace BLAZAM.Server.Data.Services
{
    public class ApplicationUserSessionCache : IApplicationUserSessionCache
    {

        private Dictionary<Type, object> _cache = new Dictionary<Type, object>();

        public T? Get<T>(Type key)
        {
            try
            {
                return _cache.Keys.Contains(key) ? (T)_cache[key] : default(T);
            }
            catch
            {
                return default(T);
            }
        }

        public void Set(Type key, object value)
        {
            _cache[key] = value;
        }

    }
}