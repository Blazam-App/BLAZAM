
namespace BLAZAM
{
    public delegate void AppDelegate();
    public delegate void AppDelegate<in T>(T value);
    public delegate void AppDelegate<in T, in T2>(T value, T2 value2);
}
