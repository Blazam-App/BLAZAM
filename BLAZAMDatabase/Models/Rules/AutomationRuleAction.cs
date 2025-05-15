using BLAZAM.Database.Models.Permissions;
using BLAZAM.Database.Models.Templates;

namespace BLAZAM.Database.Models.Rules
{
    public enum AutomationRuleActionType
    {
        ModifyField,
        SendEmail,
        Assign,
        Unassign,
        Lockout,
        Unlock,
        Disable,
        Enable,
        Move
    }
    /// <summary>
    /// An action to be executed on a rule's filtered dirctory entries
    /// </summary>
    public class AutomationRuleAction : AppDbSetBase
    {

        /// <summary>
        /// The type of action
        /// </summary>
        public AutomationRuleActionType ActionType { get; set; }

        /// <summary>
        /// All field values set for this action
        /// </summary>
        /// <remarks>
        /// Although a list, the GUI currently restricts this to one item
        /// </remarks>
        public List<AutomationRuleActionFieldValue> FieldValues { get; set; } = new();
       
        /// <summary>
        /// Dynamic JSON data to hold data for actions other than field changes or direct group assignments
        /// </summary>
        public string? Data { get; set; }
        /// <summary>
        /// All Groups to assign or unassign
        /// </summary>
        /// <remarks>
        /// Although a list, the GUI currently restricts this to one item
        /// </remarks>
        public List<AutomationRuleGroupSid> GroupSids { get; set; } = new();
        public Guid ActionGuid { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is AutomationRuleAction otherFilter)
            {
                return ActionGuid.Equals(otherFilter.ActionGuid);

            }
            return false;
        }
    }
}