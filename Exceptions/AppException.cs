namespace BLAZAM.Server.Exceptions
{
    public class AppException:Exception
    {
#pragma warning disable CS0114 // Member hides inherited member; missing override keyword
        public virtual string? Message { get; }
#pragma warning restore CS0114 // Member hides inherited member; missing override keyword
    }
}