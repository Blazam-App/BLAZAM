namespace BLAZAM.Session.Interfaces
{
    public interface IApplicationUserSessionCache
    {
        /// <summary>
        /// Returns the requests cached value/object
        /// </summary>
        /// <typeparam name="T">The type of data stored</typeparam>
        /// <param name="key">The key for the cached data</param>
        /// <returns>The cached data, or a new instance of the data type if no data is cached.</returns>
        T Get<T>(Type key) where T : new();
        void Set(Type key, object value);
    }
}