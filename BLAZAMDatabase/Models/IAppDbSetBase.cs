namespace BLAZAM.Database.Models
{
    public interface IAppDbSetBase
    {
        int Id { get; set; }

        bool Equals(AppDbSetBase? other);
        bool Equals(object? obj);
        int GetHashCode();
    }
}