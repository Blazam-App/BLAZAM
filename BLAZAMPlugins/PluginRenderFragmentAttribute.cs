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

    [AttributeUsage(AttributeTargets.Class)]
    public class PluginRenderFragmentAttribute : Attribute
    {
        public PageType PageType { get; }

        public PluginRenderFragmentAttribute(PageType pageType)
        {
            PageType = pageType;
        }
    }
}
