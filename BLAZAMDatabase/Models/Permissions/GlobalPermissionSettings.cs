using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Database.Models.Permissions
{
    public class GlobalPermissionSettings:AppDbSetBase
    {
        public bool AllowSelfModification { get; set; }
        public AccessLevel? SelfAccessLevel { get; set; }
        public int? SelfAccessLevelId { get; set; }

        public bool AllowAccessRequest { get; set; }
        public List<ObjectAction>? AllowedAccessRequestActions { get; set; }
        
    }
}
