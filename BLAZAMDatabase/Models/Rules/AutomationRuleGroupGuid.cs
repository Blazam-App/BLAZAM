using System.Text.Json.Serialization;

namespace BLAZAM.Database.Models.Rules
{
    public class AutomationRuleGroupGuid : AppDbSetBase
    {
        public Guid GroupGuid { get; set; }

        [JsonIgnore]
        public AutomationRuleAction AutomationRuleAction { get; set; }

        /// <summary>
        /// True for assign, Flase for unassign
        /// </summary>
        public bool Assigned { get; set; }

        public int AutomationRuleActionId { get; set; }
    }
}