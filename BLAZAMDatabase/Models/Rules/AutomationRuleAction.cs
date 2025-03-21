using BLAZAM.Database.Models.Permissions;

namespace BLAZAM.Database.Models.Rules
{
    public enum AutomationRuleActionType
    {
        ModifyField,
        SendEmail
    }
    public class AutomationRuleAction : AppDbSetBase
    {
        public AutomationRuleActionType ActionType { get; set; }


        public ActiveDirectoryObjectAction ActiveDirectoryObjectAction { get; set; }
        public List<AutomationRuleFieldValue> FieldValues { get; set; }
        public List<AutomationRuleGroupSid> GroupSids { get; set; }

    }
}