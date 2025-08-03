using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Plugins
{   
    /// <summary>
    /// Determines the page type that the PluginComponent will be shown
    /// </summary>
    public enum PageType
    {
        Settings,
        AllEntryTypes,
        User,
        Group,
        Contact,
        Computer,
        OU,
        Printer,
        Widget,
        Plugin,
    }
    /// <summary>
    /// Determines the location on the page that the PluginComponent should be shown
    /// </summary>
    public enum PageLocation
    {
        SubHeader,
        Details,
        Account,
        Organization,
        Profile,
        CustomFields,
        Settings,
        ContactInfo,
        Name,
    }
    /// <summary>
    /// This attribute provides the nececessary information for Blazam to include the attached Razor component
    /// in the default Blazam pages. For adding entire pages use <see cref=""/>
    /// </summary>
    public class PluginComponentAttribute : Attribute
    {
        public PageType PageType { get; }

        public PageLocation PageLocation { get; set; }

        public PluginComponentAttribute(PageType pageType, PageLocation pageLocation)
        {
            PageType = pageType;
            PageLocation = pageLocation;
        }
    }
}
