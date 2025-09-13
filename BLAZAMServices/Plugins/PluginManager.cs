using BLAZAM.Helpers;
using BLAZAM.Plugins;

namespace BLAZAM.Services.Plugins
{

    /// <summary>
    /// Manages the loading, registration, and lifecycle of external plugins.
    /// This service is intended to provide extensibility to the application. (Functionality TBD)
    /// </summary>
    public static class PluginManager
    {
        /// <summary>
        /// Finds all plugin component types intended for a specific page and location.
        /// </summary>
        /// <param name="pageType">The type of page being displayed.</param>
        /// <param name="pageLocation">The location on the page where the component will be rendered.</param>
        /// <returns>A list of component types that match the criteria.</returns>
        public static List<Type> GetPluginTypeForPageAndLocation(PageType pageType, PageLocation pageLocation)
        {
            // Scans all loaded plugins for components whose attributes match the requested page and location.
            return ApplicationInfo.loadedPlugins
                .Select(p => p.Assembly)
                .SelectMany(assembly => assembly.GetPluginComponents())
                .Where(component => component.GetPluginComponentAttributes()
                    .Any(attribute => DoesAttributeMatch(attribute, pageType, pageLocation)))
                .ToList();
        }

        /// <summary>
        /// Checks if a plugin component's attribute matches the requested page context.
        /// </summary>
        /// <param name="attribute">The attribute of the plugin component to check.</param>
        /// <param name="requestedPageType">The page type being displayed.</param>
        /// <param name="requestedPageLocation">The location on the page.</param>
        /// <returns>True if the component should be displayed.</returns>
        private static bool DoesAttributeMatch(PluginComponentAttribute attribute, PageType requestedPageType, PageLocation requestedPageLocation)
        {
            // The component's registered location must match the requested location.
            if (attribute.PageLocation != requestedPageLocation)
            {
                return false;
            }

            // A direct match of page types is always valid.
            if (attribute.PageType == requestedPageType)
            {
                return true;
            }

            // When viewing a specific entry page (e.g., User), also include components registered for 'AllEntryTypes'.
            if (IsIndividualEntryType(requestedPageType) && attribute.PageType == PageType.AllEntryTypes)
            {
                return true;
            }

            // When viewing the 'AllEntryTypes' page, include components registered for any individual entry type.
            if (requestedPageType == PageType.AllEntryTypes && IsIndividualEntryType(attribute.PageType))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// A set of page types that represent specific directory entry types.
        /// </summary>
        private static readonly HashSet<PageType> IndividualEntryTypes = new HashSet<PageType>
        {
            PageType.User, PageType.Computer, PageType.Contact,
            PageType.Printer, PageType.OU, PageType.Group
        };

        /// <summary>
        /// Determines if a page type represents a single kind of directory entry (e.g., User, Computer).
        /// </summary>
        /// <param name="pageType">The page type to check.</param>
        /// <returns>True if the page type is an individual entry type.</returns>
        private static bool IsIndividualEntryType(PageType pageType) => IndividualEntryTypes.Contains(pageType);
        /// <summary>
        /// Retrieves all <see cref="DbContext"/> types from loaded plugins that implement <see cref="IPluginDbContext"/>.
        /// </summary>
        /// <returns>A list of plugin <see cref="DbContext"/> types.</returns>
        public static List<Type> GetPluginDbContextTypes()
        {
            return ApplicationInfo.loadedPlugins
                .Select(p => p.PluginBase)
                .OfType<IPluginDbContext>()
                .Select(dbCtx => dbCtx.DbContextType)
                .ToList();
        }
        public static Type? GetPluginSettingsComponent(IPluginBase plugin)
        {
            var matchingPlugin = ApplicationInfo.loadedPlugins.FirstOrDefault(p => p.PluginBase.Equals(plugin));
            if (matchingPlugin?.Assembly != null)
            {
                var settingsPage = matchingPlugin.Assembly.GetPluginComponents();
                foreach (Type pluginRenderFragment in settingsPage)
                {
                    var attribute = pluginRenderFragment.GetPluginComponentAttributes().FirstOrDefault();
                    if (attribute != null)
                    {
                        if (attribute.PageType == PageType.Plugin && attribute.PageLocation == PageLocation.Settings)
                        {
                            return pluginRenderFragment;

                        }
                    }
                }


            }
            return null;
        }
    }
}
