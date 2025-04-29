using System.ComponentModel.DataAnnotations.Schema;

namespace BLAZAM.Database.Models.Rules
{

    public class AutomationRuleAndFilter : ActiveDirectoryFieldDbSet
    {
   
        public AutomationRuleOrFilter OrFilter { get; set; }
        public int OrFilterId { get; set; }


        public string? Value { get; set; }
        public ActiveDirectoryFieldOperator Operator { get; set; }
        /// <summary>
        /// If true, the filter should return true if the filter does not match, and false if it does
        /// </summary>
        public bool Negate { get; set; }
        public TimeSpan? TimeFrame { get; set; }

        public Guid FilterGuid { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is AutomationRuleAndFilter otherFilter){
                return FilterGuid.Equals(otherFilter.FilterGuid);

            }
            return false;
        }
       
    }
}