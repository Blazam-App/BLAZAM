using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Database.Models.Permissions
{
    public class GlobalPermissionRequestActions : AppDbSetBase
    {
        public ActiveDirectoryObjectAction ObjectAction { get; set; }

    }
}
