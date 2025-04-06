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
    public class AutomationRuleAction : AppDbSetBase
    {
        public AutomationRuleActionType ActionType { get; set; }


        public ActiveDirectoryObjectAction ActiveDirectoryObjectAction { get; set; }
        public List<AutomationRuleFieldValue> FieldValues { get; set; } = new();
       
        /// <summary>
        /// Dynamic JSON data to hold data for actions other than field changes or group assignments
        /// </summary>
        public string? Data { get; set; }
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