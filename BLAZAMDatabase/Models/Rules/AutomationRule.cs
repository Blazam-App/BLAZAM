using BLAZAM.Common.Data;
using BLAZAM.Database.Models.Notifications;
using BLAZAM.Database.Models.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Database.Models.Rules
{
    public enum ScheduleInterval
    {
        Daily,
        TimeInterval,
        Weekly,
        WeekInterval,
        Monthly,
        MonthInverval
    }
    public class AutomationRule : RecoverableAppDbSetBase
    {

        /// <summary>
        /// The name of this rule
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        /// The timestamp of the last time this Rule's filters matched an event
        /// </summary>
        /// <remarks>
        /// This is set even if the rule is disabled
        /// </remarks>
        public DateTime? LastTriggered { get; set; }

        /// <summary>
        /// The last time this rule was enabled and executed
        /// </summary>
        public DateTime? LastExcecuted { get; set; }


        /// <summary>
        /// Indicates whether or not this rule is enabled.
        /// </summary>
        public bool Enabled { get; set; }
        /// <summary>
        /// Do not continue with rules with a higher order number
        /// </summary>
        public bool StopOnThisRule { get; set; }

        /// <summary>
        /// The processing order for this rule.
        /// </summary>
        /// <remarks>
        /// Rules are processed from lowest to highest
        /// </remarks>
        public int Order { get; set; }
        /// <summary>
        /// The date at which this rule should stop executing
        /// </summary>
        public DateTime? ExpirationDate { get; set; }

        public ScheduleInterval? ScheduleInterval { get; set; }
        public TimeSpan? ScheduledRunTime { get; set; }
        public int? IntervalCount { get; set; }
        public NotificationType Trigger { get; set; }
        /// <summary>
        /// This rule will only fire for events related to these types of
        /// AD objects
        /// </summary>
        public ActiveDirectoryObjectType ActiveDirectoryObjectType { get; set; }
        /// <summary>
        /// Or List of Ands
        /// </summary>
        public List<AutomationRuleOrFilter> Filters { get; set; } = new();

        /// <summary>
        /// The actions to perform if this rule's filters are applicable to this triggering event
        /// </summary>
        public List<AutomationRuleAction> Actions { get; set; } = new() {};

        public override string ToString()
        {
            return Name??"New Rule";
        }
    }
}
