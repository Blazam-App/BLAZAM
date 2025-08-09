namespace BLAZAM.Database.Models.Rules
{
    /// <summary>
    /// An or filter which is really a collection of ands
    /// </summary>
    public class AutomationRuleOrFilter : AppDbSetBase
    {
        /// <summary>
        /// The and filters which cumulatively represent this OrFilter's value
        /// </summary>
        public List<AutomationRuleAndFilter> AndFilters { get; set; } = new();
        /// <summary>
        /// The rule that this filter belongs to
        /// </summary>
        public AutomationRule AutomationRule { get; set; }
        public int AutomationRuleId { get; set; }
        public Guid FilterGuid { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is AutomationRuleOrFilter otherFilter)
            {
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