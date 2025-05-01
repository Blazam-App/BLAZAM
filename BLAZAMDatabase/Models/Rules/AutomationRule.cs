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
        TimeInterval,
        Daily,
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

        /// <summary>
        /// The schedule interval type defining the periodicity of the schedule
        /// </summary>
        public ScheduleInterval? ScheduleInterval { get; set; }

        /// <summary>
        /// The interval time or time of day to run for scheduled rules
        /// </summary>
        public TimeSpan? ScheduledRunTime { get; set; }

        /// <summary>
        /// Used for fixed day, week, or month intervals to define repeat schedule per unit
        /// </summary>
        public int? IntervalCount { get; set; }

        /// <summary>
        /// A single byte acting as a bit array of 7 bits for flagging Sun-Sat.
        /// <para>
        /// Only used if <see cref="ScheduleInterval"/> is weekly or weekly interval.
        /// </para>
        /// </summary>
        /// <remarks>
        /// The last bit is unused.
        /// </remarks>
        public byte? DaysOfWeekToRun { get; set; }

        /// <summary>
        /// What <see cref="NotificationType"/>'s this rule will execute on
        /// </summary>
        public NotificationType Trigger { get; set; }

        /// <summary>
        /// This rule will only fire for events related to these types of
        /// AD objects
        /// </summary>
        public ActiveDirectoryObjectType ActiveDirectoryObjectType { get; set; }

        /// <summary>
        /// Or List of Ands to filter object by
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
