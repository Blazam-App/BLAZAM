namespace BLAZAM.Database.Models.Rules
{
    public class AutomationRuleGroupSid:AppDbSetBase
    {
        public string GroupSid { get; set; }
        public AutomationRuleAction AutomationRuleAction { get; set; }
        public int AutomationRuleActionId { get; set; }
    }
}