namespace BLAZAM.Common.Models.Database
{
    public class AppDbSetBase : IEquatable<AppDbSetBase?>
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