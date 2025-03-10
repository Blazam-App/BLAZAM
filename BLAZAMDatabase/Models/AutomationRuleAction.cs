namespace BLAZAM.Database.Models
{
    public class AutomationRuleAction: RecoverableAppDbSetBase
    {
        public List<AutomationRuleFieldValue> FieldValues { get; set; }

    }
}