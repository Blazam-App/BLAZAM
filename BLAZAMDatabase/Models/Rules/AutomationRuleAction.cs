using BLAZAM.Database.Models.Permissions;

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
        public List<AutomationRuleFieldValue> FieldValues { get; set; }
        public string? MoveTo { get; set; }
        public List<AutomationRuleGroupSid> GroupSids { get; set; }

    }
}