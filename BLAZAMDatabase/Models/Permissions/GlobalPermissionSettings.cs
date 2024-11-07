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

        public bool AllowAccessRequest { get; set; }
        
    }
}
