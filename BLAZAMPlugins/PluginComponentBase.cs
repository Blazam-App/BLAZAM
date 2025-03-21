using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Plugins
{
    public enum PageType
    {
        Settings
    }

    public class PluginComponentBase
    {
        public PageType PageType { get; protected set; }

       
    }
}
