using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Database.Models.Permissions
{
    public class GlobalPermissionRequestActions:AppDbSetBase
    {
        public ObjectAction ObjectAction { get; set; }
        public int ObjectActionId { get; set; }
        public GlobalPermissionSettings GlobalPermissionSettings { get; set; }
        
    }
}
