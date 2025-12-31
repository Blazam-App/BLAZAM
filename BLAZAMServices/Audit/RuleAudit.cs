using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Models.Audit;
using BLAZAM.Database.Models.Rules;
using BLAZAM.Helpers;
using Microsoft.JSInterop;
using Serilog.Events;
using System.Diagnostics;

namespace BLAZAM.Services.Audit
{
    public class RuleAudit : BaseAudit
    {
        public RuleAudit(IAppDatabaseFactory factory) : base(factory)
        {
        }


        public async Task<bool> RuleScheduled(AutomationRule rule,
            TimeSpan scheduledTime)
        {
            return await Log(rule: rule,
                eventType: AutomationRuleEventType.RuleScheduled,
                message: scheduledTime.ToString());
        }
        public async Task<bool> RuleExecutionStarted(AutomationRule rule)
        {
            return await Log(rule: rule,
                eventType: AutomationRuleEventType.RuleTriggered,
                message: rule.ScheduledRunTime?.ToString());

        }
        public async Task<bool> RuleExecutionFinished(AutomationRule rule,
            TimeSpan elapsed)
        {
            return await Log(rule: rule,
                eventType: AutomationRuleEventType.RuleFinished,
                message: elapsed.ToString());

        }
        public async Task<bool> RuleFilterEvaluated(AutomationRule rule,
            string filterJson)
        {
            return await Log(rule: rule,
                eventType: AutomationRuleEventType.FilterEvaluated,
                filterJson: filterJson);
        }
        public async Task<bool> RuleActionFailed(AutomationRule rule,
            IDirectoryEntryAdapter target,
            string actionJson,
            Exception ex)
        {
            return await Log(rule: rule,
                target: target,
                level: LogEventLevel.Error,
                eventType: AutomationRuleEventType.ActionFailed,
                actionJson: actionJson,
                stackTrace: ex.StackTrace);
        }
        public async Task<bool> RuleActionExecuted(AutomationRule rule,
            IDirectoryEntryAdapter target,
            string actionJson)
        {
            return await Log(rule: rule,
                target: target,
                eventType: AutomationRuleEventType.ActionExecuted,
                actionJson: actionJson);
        }
        public async Task<bool> RuleMatchSkipped(AutomationRule rule,
            IDirectoryEntryAdapter skippedEntry)
        {
            return await Log(rule: rule,
                eventType: AutomationRuleEventType.EntrySkipped,
                level: LogEventLevel.Warning,
                target: skippedEntry);
        }
        private async Task<bool> Log(AutomationRule rule,

            AutomationRuleEventType eventType,
            LogEventLevel level = LogEventLevel.Information,
            string? message = null,
            IDirectoryEntryAdapter? target = null,
            string? stackTrace = null,
            string? actionJson = null,
            string? filterJson = null)
        {
            try
            {
                using var context = await factory.CreateDbContextAsync();
                context.AutomationRuleAuditLog.Add(new AutomationRuleAuditLog
                {
                    AutomationRuleId = rule.Id,
                    EventType = eventType,
                    Level = level,
                    Message = message,
                    ExecutionId= rule.ExecutionId,
                    TargetGuid = target?.Guid.ToString(),
                    Timestamp = DateTime.Now,
                    StackTrace = stackTrace,
                    ActionSnapshot = actionJson,
                    RuleSnapshot = rule.ToJson(),
                    FilterSnapshot = filterJson,


                });
                await context.SaveChangesAsync();
                return true;

            }
            catch
            {
                return false;
            }
        }
    }
}