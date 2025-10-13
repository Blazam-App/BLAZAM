namespace BLAZAM.Database.Models.Rules
{
    public class GlobalAutomationRuleSettings : AppDbSetBase
    {
        public List<AutomationRuleExcludedGroupSid> ExcludedGroups { get; set; } = [];


    }
}
