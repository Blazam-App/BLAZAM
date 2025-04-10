
namespace BLAZAM
{
    public delegate void AppDelegate();
    public delegate void AppDelegate<T>(T value);
    public delegate void AppDelegate<T, T2>(T value, T2 value2);
}
