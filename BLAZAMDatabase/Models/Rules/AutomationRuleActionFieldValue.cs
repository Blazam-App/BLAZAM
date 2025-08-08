namespace BLAZAM.Database.Models.Rules
{
    public class AutomationRuleActionFieldValue : ActiveDirectoryFieldDbSet
    {

        public string? Value { get; set; }
        public AutomationRuleAction AutomationRuleAction { get; set; }
        public int AutomationRuleActionId { get; set; }
    }
}