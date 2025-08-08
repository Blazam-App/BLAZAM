namespace BLAZAM.Session.Interfaces
{
    /// <summary>
    /// Defines a contract for a simple in-memory cache for storing user session-specific data.
    /// </summary>
    public interface IApplicationUserSessionCache
    {
        /// <summary>
        /// Retrieves a cached object by its Type key.
        /// </summary>
        /// <typeparam name="T">The expected type of the cached object.</typeparam>
        /// <param name="key">The Type used as the key.</param>
        /// <returns>The cached object or a new instance of T if not found or on error.</returns>
        T Get<T>(Type key) where T : new();

        /// <summary>
        /// Retrieves a cached object by its string key.
        /// </summary>
        /// <typeparam name="T">The expected type of the cached object.</typeparam>
        /// <param name="key">The string key.</param>
        /// <returns>The cached object or a new instance of T if not found or on error.</returns>
        T Get<T>(string key) where T : new();

        /// <summary>
        /// Sets or updates a cached object by its Type key.
        /// </summary>
        /// <param name="key">The Type key.</param>
        /// <param name="value">The object to cache.</param>
        void Set(Type key, object value);

        /// <summary>
        /// Sets or updates a cached object by its string key.
        /// </summary>
        /// <param name="key">The string key.</param>
        /// <param name="value">The object to cache.</param>
        void Set(string key, object value);
    }
}