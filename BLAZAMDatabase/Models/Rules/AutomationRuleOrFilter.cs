namespace BLAZAM.Database.Models.Rules
{
    public class AutomationRuleOrFilter:AppDbSetBase
    {
        public List<AutomationRuleAndFilter> AndFilters { get; set; }
        public AutomationRule AutomationRule { get; set; }
        public int AutomationRuleId { get; set; }
    }
}