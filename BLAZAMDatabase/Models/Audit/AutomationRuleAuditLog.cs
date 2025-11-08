using BLAZAM.Database.Models.Notifications;
using BLAZAM.Database.Models.Rules;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Database.Models.Audit
{
    public enum AutomationRuleEventType
    {
        RuleTriggered,
        ActionExecuted,
        ActionFailed,
        FilterEvaluated,
        RuleScheduled,
        RuleFinished,
        EntrySkipped
    }
    public class AutomationRuleAuditLog : AppDbSetBase
    {
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public AutomationRuleEventType EventType { get; set; }
        public AutomationRule AutomationRule { get; set; }
        public int AutomationRuleId { get; set; }
        public LogEventLevel Level { get; set; } = LogEventLevel.Information;
        public string? TargetGuid { get; set; }
        public string? Message { get; set; }
        public string? StackTrace { get; set; } 
        public string? RuleSnapshot { get; set; }
        public string? ActionSnapshot { get; set; }
        public string? FilterSnapshot { get; set; }
        public bool? MatchesFilter { get; set; }
        public NotificationType? Trigger { get; set; }



    }
}
