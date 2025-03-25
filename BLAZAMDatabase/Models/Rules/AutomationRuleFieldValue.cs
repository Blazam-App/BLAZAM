namespace BLAZAM.Database.Models.Rules
{
    public class AutomationRuleFieldValue : AppDbSetBase
    {
        public ActiveDirectoryField Field { get; set; }
        public string? Value { get; set; }
        public AutomationRuleAction AutomationRuleAction { get; set; }
        public int AutomationRuleActionId { get; set; }
    }
}