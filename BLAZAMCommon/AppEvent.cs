
namespace BLAZAM
{
    public delegate void AppEvent();
    public delegate void AppEvent<T>(T value);
    public delegate void AppEvent<T, T2>(T value, T2 value2);
}
