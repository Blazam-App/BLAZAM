namespace BLAZAM.Database.Models
{
    /// <summary>
    /// Base class that all Database Model classes should inherit
    /// </summary>
    /// <remarks>
    /// Provides an <see cref="Id"/> as well as <see cref="Equals(object?)"/> and
    /// <see cref="GetHashCode"/>
    /// </remarks>
    public class AppDbSetBase : IEquatable<AppDbSetBase?>, IAppDbSetBase
    {
        public int Id { get; set; }

        public override bool Equals(object? obj)
        {
            return Equals(obj as AppDbSetBase);
        }

        public bool Equals(AppDbSetBase? other)
        {
            return other is not null &&
                   Id == other.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }

        public static bool operator ==(AppDbSetBase? left, AppDbSetBase? right)
        {
            return EqualityComparer<AppDbSetBase>.Default.Equals(left, right);
        }

        public static bool operator !=(AppDbSetBase? left, AppDbSetBase? right)
        {
            return !(left == right);
        }
    }
}