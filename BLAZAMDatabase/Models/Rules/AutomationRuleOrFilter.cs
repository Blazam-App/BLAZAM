namespace BLAZAM.Database.Models.Rules
{
    public class AutomationRuleOrFilter:AppDbSetBase
    {

        public List<AutomationRuleAndFilter> AndFilters { get; set; } = new();
        public AutomationRule AutomationRule { get; set; }
        public int AutomationRuleId { get; set; }
        public Guid FilterGuid { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is AutomationRuleOrFilter otherFilter){
                return FilterGuid.Equals(otherFilter.FilterGuid);

            }
            return false;
        }

        public override int GetHashCode()
        {
            return FilterGuid.GetHashCode();
        }
    }
}