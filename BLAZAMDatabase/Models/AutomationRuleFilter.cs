namespace BLAZAM.Database.Models
{
    public enum RuleOperator
    {
        Equals,
        Contains,
        StartsWith,
        PreviousTimeFrame,
        FutureTimeFrame,
        BeforeNow,

    }
    public class AutomationRuleFilter:RecoverableAppDbSetBase
    {
        public ActiveDirectoryField Field { get; set; }
        public object? Value { get; set; }
        public RuleOperator Operator { get; set; }
        /// <summary>
        /// If true, the filter should return true if the filter does not match, and false if it does
        /// </summary>
        public bool Negate { get; set; }
        public TimeSpan? TimeFrame { get; set; }

    }
}