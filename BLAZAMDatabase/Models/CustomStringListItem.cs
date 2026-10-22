namespace BLAZAM.Database.Models
{
    public class CustomStringListItem : AppDbSetBase
    {
        public int CustomStringListId { get; set; }
        public CustomStringList? CustomStringList { get; set; }

        public string Value { get; set; } = string.Empty;

        public override string? ToString()
        {
            return Value;
        }
    }
}
