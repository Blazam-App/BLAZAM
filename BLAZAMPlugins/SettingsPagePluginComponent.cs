using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Plugins
{

    public class SettingsPagePluginComponent : PluginComponentBase
    {
        public SettingsPagePluginComponent()
        {
            PageType= PageType.Settings;
        }
    }
}
