namespace BLAZAM.Database.Models.Rules
{
    public class GlobalAutomationRuleSettings : AppDbSetBase
    {
        public List<AutomationRuleExcludedGroupGuid> ExcludedGroups { get; set; } = [];


    }
}
