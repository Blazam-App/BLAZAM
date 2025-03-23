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
    public class AutomationRule : RecoverableAppDbSetBase
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public bool StopOnThisRule { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public NotificationType Trigger { get; set; }
        public ActiveDirectoryObjectType ActiveDirectoryObjectType { get; set; }
        /// <summary>
        /// Or List of Ands
        /// </summary>
        public List<AutomationRuleOrFilter> Filters { get; set; } = new();
        public List<AutomationRuleAction> Actions { get; set; } = new();
    }
}
