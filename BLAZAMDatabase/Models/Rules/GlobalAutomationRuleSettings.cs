using System.ComponentModel;
using System.ComponentModel.Design.Serialization;

namespace BLAZAM.Database.Models.Rules
{
    /// <summary>
    /// Represents the global settings for automation rules, including their enabled state and excluded groups.
    /// </summary>
    /// <remarks>This class provides configuration options for managing automation rules at a global level.
    /// Use the <see cref="RulesEnabled"/> property to enable or disable automation rules globally, and the <see
    /// cref="ExcludedGroups"/> property to specify groups that should be excluded from the rules.</remarks>
    public class GlobalAutomationRuleSettings : AppDbSetBase
    {
        /// <summary>
        /// Gets or sets a value indicating whether rules are enabled.
        /// </summary>
        [DefaultValue(true)]
        public bool RulesEnabled { get; set; } = true;
        /// <summary>
        /// Gets or sets the list of groups that are excluded from the automation rule. 
        /// </summary>
        /// <remarks>Use this property to specify groups that should not be affected by the automation
        /// rule.</remarks>
        public List<AutomationRuleExcludedGroupGuid> ExcludedGroups { get; set; } = [];


    }
}
