using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Plugins
{
    public enum PageType
    {
        Settings,
        User,
        Group,
        Computer,
        OU,
        Printer,
        Widget
    }

    public class PluginRenderFragmentAttribute : Attribute
    {
        public PageType PageType { get; }

        public PluginRenderFragmentAttribute(PageType pageType)
        {
            PageType = pageType;
        }
    }
}
