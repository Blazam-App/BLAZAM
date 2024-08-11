using BLAZAM.Common.Data.Services;
using BLAZAM.Session.Interfaces;

namespace BLAZAM.Server.Data.Services
{
    public class ApplicationUserSessionCache : IApplicationUserSessionCache
    {

        private Dictionary<Type, object> _typeCache = new Dictionary<Type, object>();
        private Dictionary<string, object> _stringCache = new Dictionary<string, object>();

        public T Get<T>(Type key) where T : new()
        {
            try
            {
                return _typeCache.Keys.Contains(key) ? (T)_typeCache[key] : new T();
            }
            catch
            {
                return new T();
            }
        }

        public void Set(Type key, object value)
        {
            _typeCache[key] = value;
        }
        public T Get<T>(string key) where T : new()
        {
            try
            {
                return _stringCache.Keys.Contains(key) ? (T)_stringCache[key] : new T();
            }
            catch
            {
                return new T();
            }
        }

        public void Set(string key, object value)
        {
            _stringCache[key] = value;
        }
    }
}