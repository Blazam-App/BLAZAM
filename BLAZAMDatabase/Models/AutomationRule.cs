using BLAZAM.Database.Models.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Database.Models
{
    public class AutomationRule
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public ActiveDirectoryObjectAction Trigger { get; set; }
    }
}
