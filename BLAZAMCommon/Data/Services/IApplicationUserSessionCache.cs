namespace BLAZAM.Common.Data.Services
{
    public interface IApplicationUserSessionCache
    {
        T? Get<T>(Type key);
        void Set(Type key, object value);
    }
}