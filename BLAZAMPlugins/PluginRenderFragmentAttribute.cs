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
        Widget,
        Plugin,
    }
    public enum PageLocation
    {
        SubHeader,
        Details,
        Account,
        Organization,
        Profile,
        CustomFields,
        Settings,
    }

    public class PluginRenderFragmentAttribute : Attribute
    {
        public PageType PageType { get; }

        public PageLocation PageLocation { get; set; }

        public PluginRenderFragmentAttribute(PageType pageType, PageLocation pageLocation)
        {
            PageType = pageType;
            PageLocation = pageLocation;
        }
    }
}
