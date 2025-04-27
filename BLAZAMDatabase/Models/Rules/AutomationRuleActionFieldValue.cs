namespace BLAZAM.Database.Models.Rules
{
    public class AutomationRuleActionFieldValue : AppDbSetBase
    {
        public ActiveDirectoryField? Field { get; set; }
        public int? FieldId { get; set; }
        public CustomActiveDirectoryField? CustomField { get; set; }
        public int? CustomFieldId { get; set; }
        public string? Value { get; set; }
        public AutomationRuleAction AutomationRuleAction { get; set; }
        public int AutomationRuleActionId { get; set; }
    }
}