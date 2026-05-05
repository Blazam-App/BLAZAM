using BLAZAM.Logger; // Added
using BLAZAM.Session.Interfaces;

namespace BLAZAM.Session
{
    /// <summary>
    /// Provides a simple in-memory, per-user-session cache for storing arbitrary data, keyed by Type or string.
    /// </summary>
    public class ApplicationUserSessionCache : IApplicationUserSessionCache
    {
        private readonly Dictionary<Type, object> _typeCache = [];
        private readonly Dictionary<string, object> _stringCache = [];
        private bool _disposedValue;

        /// <summary>
        /// Retrieves a cached object by its Type key.
        /// </summary>
        /// <typeparam name="T">The expected type of the cached object. A new instance of this type is required via `new()` constraint.</typeparam>
        /// <param name="key">The Type used as the key for the cached data.</param>
        /// <returns>The cached object cast to type T. Returns a new instance of T if the key is not found or if an error occurs during retrieval.</returns>
        public T Get<T>(Type key) where T : new()
        {
            try
            {
                return _typeCache.Keys.Contains(key) ? (T)_typeCache[key] : new T();
            }
            catch (Exception ex) // Catch specific Exception 'ex'
            {
                Loggers.SystemLogger.Debug(ex, "ApplicationUserSessionCache.Get (by Type): Exception while trying to retrieve cache item with key {CacheKey}. Returning new T().", key);
                return new T();
            }
        }

        /// <summary>
        /// Retrieves a cached object by its string key.
        /// </summary>
        /// <typeparam name="T">The expected type of the cached object. A new instance of this type is required via `new()` constraint.</typeparam>
        /// <param name="key">The string key for the cached data.</param>
        /// <returns>The cached object cast to type T. Returns a new instance of T if the key is not found or if an error occurs during retrieval.</returns>
        public T Get<T>(string key) where T : new()
        {
            try
            {
                return _stringCache.Keys.Contains(key) ? (T)_stringCache[key] : new T();
            }
            catch (Exception ex) // Catch specific Exception 'ex'
            {
                Loggers.SystemLogger.Debug(ex, "ApplicationUserSessionCache.Get (by string): Exception while trying to retrieve cache item with key {CacheKey}. Returning new T().", key);
                return new T();
            }
        }

        /// <summary>
        /// Sets or updates a cached object identified by its Type key.
        /// </summary>
        /// <param name="key">The Type to use as the key.</param>
        /// <param name="value">The object to cache.</param>
        public void Set(Type key, object value)
        {
            _typeCache[key] = value;
        }

        /// <summary>
        /// Sets or updates a cached object identified by its string key.
        /// </summary>
        /// <param name="key">The string to use as the key.</param>
        /// <param name="value">The object to cache.</param>
        public void Set(string key, object value)
        {
            _stringCache[key] = value;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects)
                    foreach (var item in _typeCache.Values
                        .Where(x => x is IDisposable)
                        .Cast<IDisposable>())
                    {
                        item.Dispose();
                    }
                    _typeCache.Clear();

                    foreach (var item in _stringCache.Values
                        .Where(x => x is IDisposable)
                        .Cast<IDisposable>())
                    {
                        item.Dispose();
                    }
                    _stringCache.Clear();
                }

                // No unmanaged resources to free

                _disposedValue = true;
            }
        }



        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}