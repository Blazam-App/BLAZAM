namespace BLAZAM.Database.Models
{
    public class AutomationRuleFieldValue: AppDbSetBase
    {
        public ActiveDirectoryField Field { get; set; }
        public object? Value { get; set; }
    }
}