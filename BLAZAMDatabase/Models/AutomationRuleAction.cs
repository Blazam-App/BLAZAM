using BLAZAM.Database.Models.Permissions;

namespace BLAZAM.Database.Models
{

    public class AutomationRuleAction: RecoverableAppDbSetBase
    {
    
        public ActiveDirectoryObjectAction ActionType { get; set; }
        public List<AutomationRuleFieldValue> FieldValues { get; set; }

    }
}